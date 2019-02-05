using RestSharp;
using Newtonsoft.Json;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Xceed.Wpf.AvalonDock;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NasuniConnections
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class MonitoringStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
        String rString = "";
        if ((Boolean)((NasuniFiler)value).Offline)
        {
            rString = "Images/Oxygen-Icons.org-Oxygen-Actions-edit-delete.ico";
        }
        else
        {            
            rString = "Images/Oxygen-Icons.org-Oxygen-Actions-dialog-ok-apply.ico";
        }
        return rString;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class MainWindow : Window
    {
        private ObservableCollection<NasuniFiler> nasunifilers = new ObservableCollection<NasuniFiler>();
        private ObservableCollection<ADSite> adsites = new ObservableCollection<ADSite>();
        private ObservableCollection<SearchResult> searchresults = new ObservableCollection<SearchResult>();
        private ObservableCollection<SearchResult> filteredresults = new ObservableCollection<SearchResult>();
        private Dictionary<String, String> subnetlist = new Dictionary<string, string>();
        private bool sitesloaded = false;
        private string authtoken = "";
        private string domain = "domain.com";
        private string nmc = "https://nasuni.domain.com";
        private string nasuniusername = "nasuniusername";
        private string nasunipassword = "nasunipassword";
        private dynamic filers;
        public MainWindow()
        {
            
            InitializeComponent();     
            textblockVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //Disable Search Tab
            tabSearch.IsEnabled = false;

            //Load the NMC and Domain Information from settings
            textboxNMC.Text = nmc = Properties.Settings.Default.nmc;
            textboxDomain.Text = domain = Properties.Settings.Default.domainname;
            textboxNasuniUser.Text = nasuniusername = Properties.Settings.Default.nasuniusername;
            nasunipassword = Properties.Settings.Default.nasunipassword;


            //Check if a valid nmc is in the settings
            Regex nmcregex = new Regex(@"https://\w+\.\w+\.\w+");
            Match nmcmatch = nmcregex.Match(nmc);
            if(nmcmatch.Success)
            {
                GenerateToken();
            }
            statusbarStatus.Content = "Click \'Load Filers\' to start.";
        }
        private void GenerateToken()
        {
            try
            {
                //Generate Auth Token
                var client = new RestClient($"{nmc}/api/v1.1/auth/login/");
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
                request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW", $"------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"username\"\r\n\r\n{nasuniusername}\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"password\"\r\n\r\n{nasunipassword}\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                dynamic responsetoken = JsonConvert.DeserializeObject(response.Content);
                authtoken = "token " + responsetoken.token;
            }
            catch
            {
                buttonLoadFilers.IsEnabled = false;
                MessageBox.Show("Please configure console settings under File -> Settings", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadFilers()
        {
            if(authtoken == null)
            {
                GenerateToken();
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Generate list of AD Subnets
            try
            {                
                subnetlist.Clear();
                adsites.Clear();

                DirectoryContext adcontext = new DirectoryContext(DirectoryContextType.Forest, domain);
                Forest adforest = Forest.GetForest(adcontext);
                foreach (ActiveDirectorySite site in adforest.Sites)
                {
                    ADSite newadsite = new ADSite
                    {
                        Sitename = site.Name
                    };
                    foreach (ActiveDirectorySubnet subnet in site.Subnets)
                    {
                        newadsite.Subnets.Add(subnet.Name);
                        subnetlist.Add(subnet.Name, site.Name);
                    }
                    adsites.Add(newadsite);
                }
            }
            catch
            {
                MessageBox.Show($"Please configure domain[{domain}] settings under File -> Settings", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            nasunifilers.Clear();
            //Generate list of Nasuni Filers
            var client = new RestClient($"{nmc}/api/v1.1/filers/");
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", authtoken);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            IRestResponse response = client.Execute(request);

            filers = JsonConvert.DeserializeObject(response.Content);
            foreach (dynamic filer in filers.items)
            {
                BasicInformation newinfo = new BasicInformation
                {
                    Build = filer.build,
                    Description = filer.description,
                    Guid = filer.guid,
                    MgmtState = filer.management_state,
                    SerialNo = filer.serial_number,
                    Timezone = filer.settings.time.timezone
                };
                CIFSSettings newcifs = new CIFSSettings
                {
                    SMB = filer.settings.cifs.smb3,
                    VetoFiles = filer.settings.cifs.veto_files,
                    AioSupport = filer.settings.cifs.aio_support,
                    SmbEncrypt = filer.settings.cifs.smb_encrypt,
                    FruitSupport = filer.settings.cifs.fruit_support,
                    DenyAccess = filer.settings.cifs.deny_access,
                    ProtoLevel = filer.settings.cifs.proto_level,
                    UnixExt = filer.settings.cifs.unix_ext,
                    Anonymous = filer.settings.cifs.restrict_anonymous
                };
                Status newstatus = new Status
                {
                    Offline = filer.status.offline,
                    Version = filer.status.osversion,
                    Platform = filer.status.platform.platform_name,
                    CacheSize = filer.status.platform.cache_status.size,
                    CacheUsed = filer.status.platform.cache_status.used,
                    CacheFree = filer.status.platform.cache_status.free,
                    PercentFree = filer.status.platform.cache_status.percent_used,
                    CpuCores = filer.status.platform.cpu.cores,
                    CpuModel = filer.status.platform.cpu.model,
                    CpuFreq = filer.status.platform.cpu.frequency,
                    Sockets = filer.status.platform.cpu.sockets,
                    Memory = filer.status.platform.memory,
                    UpdateAvail = filer.status.updates.updates_available,
                    CurrentVersion = filer.status.updates.current_version,
                    NewVersion = filer.status.updates.new_version,
                    Uptime = filer.status.uptime
                };
                NasuniFiler newfiler = new NasuniFiler
                {
                    Name = filer.description,
                    Guid = filer.guid,
                    Serialno = filer.serial_number,
                    Build = filer.build,
                    PercentUsed = Math.Round((Double)filer.status.platform.cache_status.percent_used, 2),
                    Offline = filer.status.offline,
                    BasicInfo = newinfo,
                    CifsInfo = newcifs,
                    FilerStatus = newstatus
                };
                nasunifilers.Add(newfiler);
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}.{1:00}",ts.Seconds,ts.Milliseconds / 10);
            listviewFilers.ItemsSource = nasunifilers;
            buttonLoadFilers.IsEnabled = false;
            buttonRefreshFilers.IsEnabled = true;
            tabSearch.IsEnabled = true;
            if(sitesloaded)
            {
                statusbarStatus.Content = $"Nasuni Filer Refresh Completed in {elapsedTime} Seconds";
            }
            else
            {
                statusbarStatus.Content = $"Nasuni Filer Load Completed in {elapsedTime} Seconds";
                sitesloaded = true;
            }
            
        }
        private void ListviewFilers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listviewFilers.SelectedItem != null)
            {
                NasuniFiler selectedfiler = (NasuniFiler)listviewFilers.SelectedItem;
                ObservableCollection<NasuniCIFS> cifslist = new ObservableCollection<NasuniCIFS>();
                ObservableCollection<NasuniLock> lockslist = new ObservableCollection<NasuniLock>();
                ObservableCollection<NasuniFiler> filerlist = new ObservableCollection<NasuniFiler>();
                ObservableCollection<CifsLock> lockinfolist = new ObservableCollection<CifsLock>();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                filerlist.Add(selectedfiler);
                //Generate expander for filer
                listboxFiler.ItemsSource = filerlist;
                //Generate list of cifsclient
                var client = new RestClient($"{nmc}/api/v1/filers/{selectedfiler.Serialno}/cifsclients/");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", authtoken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                IRestResponse response = client.Execute(request);
                dynamic cifsusers = JsonConvert.DeserializeObject(response.Content);
                foreach (dynamic cifsuser in cifsusers.cifs_clients)
                {
                    NasuniCIFS newcifs = new NasuniCIFS
                    {
                        Share = cifsuser.share,
                        User = cifsuser.user,
                        Client = cifsuser.client,
                        Workstation = cifsuser.client_name
                    };
                    cifslist.Add(newcifs);
                }

                //Generate list of cifslocks
                var lockclient = new RestClient($"{nmc}/api/v1/filers/{selectedfiler.Serialno}/cifslocks/");
                var lockrequest = new RestRequest(Method.GET);
                lockrequest.AddHeader("Authorization", authtoken);
                lockrequest.AddHeader("cache-control", "no-cache");
                lockrequest.AddHeader("accept", "application/json");
                lockrequest.AddHeader("content-type", "application/json");
                IRestResponse lockresponse = lockclient.Execute(lockrequest);
                dynamic cifslocks = JsonConvert.DeserializeObject(lockresponse.Content);
                foreach (dynamic cifslock in cifslocks.cifs_locks)
                {
                    bool addnewlock = true;
                    CifsLockInfo newlockinfo = new CifsLockInfo
                    {
                        Client = cifslock.client,
                        Share = cifslock.share,
                        Type = cifslock.type,
                        FilePath = cifslock.file_path
                    };
                    foreach (CifsLock lockcompare in lockinfolist)
                    {
                        if (cifslock.user == lockcompare.Username)
                        {
                            lockcompare.CifsLockList.Add(newlockinfo);
                            addnewlock = false;
                        }
                    }
                    if (addnewlock)
                    {
                        CifsLock newcifslock = new CifsLock
                        {
                            Username = cifslock.user
                        };
                        newcifslock.CifsLockList.Add(newlockinfo);
                        lockinfolist.Add(newcifslock);
                    }
                    NasuniLock newlock = new NasuniLock
                    {
                        Share = cifslock.share,
                        FilePath = cifslock.file_path,
                        Type = cifslock.type,
                        User = cifslock.user,
                        Client = cifslock.client,
                        ClientName = cifslock.client_name
                    };
                    lockslist.Add(newlock);
                }
                #region api v1.1 pagination
                //v1.1 api pagination
                /*while (cifsusers.next != null)
                {
                    Console.WriteLine("Processing 50 users");
                    foreach (dynamic cifsuser in cifsusers.items)
                    {
                        string sharename = GetShareName((String)cifsuser.share_id);
                        NasuniCIFS newcifs = new NasuniCIFS
                        {
                            Share = sharename,
                            User = cifsuser.user,
                            Client = cifsuser.client,
                            Workstation = cifsuser.client_name
                        };
                        cifslist.Add(newcifs);
                    }
                    Console.WriteLine("Done Processing, Query Next Page");
                    if (cifsusers.next != null)
                    {
                        client = new RestClient(cifsusers.next.Value);
                        request = new RestRequest(Method.GET);
                        request.AddHeader("Authorization", authtoken);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddHeader("accept", "application/json");
                        request.AddHeader("content-type", "application/json");
                        response = client.Execute(request);
                        cifsusers = JsonConvert.DeserializeObject(response.Content);
                    }
                }
                //Check if there is only one page
                if(cifsusers.items.Count <= 50 && cifsusers.next == null && cifsusers.previous == null)
                {
                    Console.WriteLine("Processing Only Page");
                    foreach (dynamic cifsuser in cifsusers.items)
                    {
                        string sharename = GetShareName((String)cifsuser.share_id);
                        NasuniCIFS newcifs = new NasuniCIFS
                        {
                            Share = sharename,
                            User = cifsuser.user,
                            Client = cifsuser.client,
                            Workstation = cifsuser.client_name
                        };
                        cifslist.Add(newcifs);
                    }
                }
                //Check if there were multiple pages and add the stuff from the last page
                if (cifsusers.items.Count > 0 && cifsusers.next == null && cifsusers.previous != null)
                {
                    Console.WriteLine("Processing Last Page");
                    foreach (dynamic cifsuser in cifsusers.items)
                    {
                        string sharename = GetShareName((String)cifsuser.share_id);
                        NasuniCIFS newcifs = new NasuniCIFS
                        {
                            Share = sharename,
                            User = cifsuser.user,
                            Client = cifsuser.client,
                            Workstation = cifsuser.client_name
                        };
                        cifslist.Add(newcifs);
                    }
                }*/
                #endregion

                foreach (NasuniCIFS cifsclient in cifslist)
                {
                    IPAddress clientip = IPAddress.Parse(cifsclient.Client);
                    foreach (KeyValuePair<String, String> subnet in subnetlist)
                    {
                        IPNetwork ipsubnet = IPNetwork.Parse(subnet.Key);
                        if (clientip.IsInSameSubnet(ipsubnet.Network, ipsubnet.Netmask))
                        {
                            cifsclient.ConnectedSite = subnet.Value;
                            break;
                        }
                    }
                }
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
                statusbarStatus.Content = $"{selectedfiler.Name} Loaded in {elapsedTime} Seconds";
                gridCIFS.ItemsSource = cifslist;
                listboxCifsLocks.ItemsSource = lockinfolist;
                layoutCifsConnections.IsActive = true;
            }
        }
        private string GetShareName(String shareid)
        {
            var client = new RestClient($"{nmc}/api/v1.1/volumes/filers/shares/{shareid}/");
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", authtoken);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            IRestResponse response = client.Execute(request);
            dynamic shares = JsonConvert.DeserializeObject(response.Content);
            return shares.name;
        }
        private string GetFilerBySerial(String serialno)
        {
            var client = new RestClient($"{nmc}/api/v1.1/filers/{serialno}/");
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", authtoken);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            IRestResponse response = client.Execute(request);
            dynamic filers = JsonConvert.DeserializeObject(response.Content);
            return filers.description;
        }
        private string AdSiteByIP(String ipstring)
        {
            string rval = "";
            IPAddress clientip = IPAddress.Parse(ipstring);
            foreach (KeyValuePair<String, String> subnet in subnetlist)
            {
                IPNetwork ipsubnet = IPNetwork.Parse(subnet.Key);
                if (clientip.IsInSameSubnet(ipsubnet.Network, ipsubnet.Netmask))
                {
                    rval = subnet.Value;
                    break;
                }
            }
            return rval;
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            LoadFilers();
        }
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.nmc = nmc = textboxNMC.Text;
            Properties.Settings.Default.domainname = domain = textboxDomain.Text;
            Properties.Settings.Default.nasuniusername = nasuniusername = textboxNasuniUser.Text;
            Properties.Settings.Default.nasunipassword = nasunipassword = passwordboxNasuni.Password;
            Properties.Settings.Default.Save();
            GenerateToken();
            buttonLoadFilers.IsEnabled = true;
        }
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ButtonUserSearch_Click(object sender, RoutedEventArgs e)
        {
            searchresults.Clear();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Escape dots in username
            String formattedsearch = textboxUserSearch.Text.Replace(".", "\\.");
            Regex userregex = new Regex(formattedsearch);
            //Generate list of user connections
            var cifsclient = new RestClient($"{nmc}/api/v1/filers/cifsclients/");
            var cifsrequest = new RestRequest(Method.GET);
            cifsrequest.AddHeader("Authorization", authtoken);
            cifsrequest.AddHeader("cache-control", "no-cache");
            cifsrequest.AddHeader("accept", "application/json");
            cifsrequest.AddHeader("content-type", "application/json");
            IRestResponse cifsresponse = cifsclient.Execute(cifsrequest);
            dynamic cifsconnections = JsonConvert.DeserializeObject(cifsresponse.Content);
            foreach (dynamic cifsconnection in cifsconnections.items)
            {
                foreach (dynamic cifs_client in cifsconnection.cifs_clients)
                {
                    string formatteduser = cifs_client.user;
                    Match usermatch = userregex.Match(formatteduser.Replace("\\", "\\\\"));
                    if (usermatch.Success)
                    {
                        SearchResult newresult = new SearchResult
                        {
                            Share = cifs_client.share,
                            User = cifs_client.user,
                            Client = cifs_client.client,
                            ClientName = cifs_client.client_name,
                            ADSite = AdSiteByIP((string)cifs_client.client),
                            Filer = GetFilerBySerial((string)cifsconnection.serial_number)
                        };
                        searchresults.Add(newresult);
                    }
                }
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
            statusbarStatus.Content = $"Search Completed in {elapsedTime} Seconds";
            gridSearchResults.ItemsSource = searchresults;
            layoutSearchInfo.IsActive = true;
        }
        private void ButtonShareSearch_Click(object sender, RoutedEventArgs e)
        {
            searchresults.Clear();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Generate list of user connections
            var cifsclient = new RestClient($"{nmc}/api/v1/filers/cifsclients/");
            var cifsrequest = new RestRequest(Method.GET);
            cifsrequest.AddHeader("Authorization", authtoken);
            cifsrequest.AddHeader("cache-control", "no-cache");
            cifsrequest.AddHeader("accept", "application/json");
            cifsrequest.AddHeader("content-type", "application/json");
            IRestResponse cifsresponse = cifsclient.Execute(cifsrequest);
            dynamic cifsconnections = JsonConvert.DeserializeObject(cifsresponse.Content);
            foreach (dynamic cifsconnection in cifsconnections.items)
            {
                foreach (dynamic cifs_client in cifsconnection.cifs_clients)
                {
                    if ((((string)cifs_client.share).ToUpper()).Contains((textboxShareSearch.Text).ToUpper()))
                    {
                        SearchResult newresult = new SearchResult
                        {
                            Share = cifs_client.share,
                            User = cifs_client.user,
                            Client = cifs_client.client,
                            ClientName = cifs_client.client_name,
                            ADSite = AdSiteByIP((string)cifs_client.client),
                            Filer = cifsconnection.description
                        };
                        searchresults.Add(newresult);
                    }
                }
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
            statusbarStatus.Content = $"Search Completed in {elapsedTime} Seconds";
            gridSearchResults.ItemsSource = searchresults;
            layoutSearchInfo.IsActive = true;
        }
        private void TextBoxUserFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            filteredresults.Clear();
            if (searchresults.Count > 0)
            {
                foreach(SearchResult result in searchresults)
                {
                    if (result.User.ToUpper().Contains(textboxUserFilter.Text.ToUpper()))
                    {
                        filteredresults.Add(result);
                    }
                }
                gridSearchResults.ItemsSource = filteredresults;
                layoutSearchInfo.IsActive = true;
            }
        }
        private void ButtonSiteSearch_Click(object sender, RoutedEventArgs e)
        {
            searchresults.Clear();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Generate list of user connections
            var cifsclient = new RestClient($"{nmc}/api/v1/filers/cifsclients/");
            var cifsrequest = new RestRequest(Method.GET);
            cifsrequest.AddHeader("Authorization", authtoken);
            cifsrequest.AddHeader("cache-control", "no-cache");
            cifsrequest.AddHeader("accept", "application/json");
            cifsrequest.AddHeader("content-type", "application/json");
            IRestResponse cifsresponse = cifsclient.Execute(cifsrequest);
            dynamic cifsconnections = JsonConvert.DeserializeObject(cifsresponse.Content);
            foreach (dynamic cifsconnection in cifsconnections.items)
            {
                foreach (dynamic cifs_client in cifsconnection.cifs_clients)
                {
                    if (((AdSiteByIP((string)cifs_client.client)).ToUpper()).Contains((textboxSiteSearch.Text).ToUpper()))
                    {
                        SearchResult newresult = new SearchResult
                        {
                            Share = cifs_client.share,
                            User = cifs_client.user,
                            Client = cifs_client.client,
                            ClientName = cifs_client.client_name,
                            ADSite = AdSiteByIP((string)cifs_client.client),
                            Filer = cifsconnection.description
                        };
                        searchresults.Add(newresult);
                    }
                }
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
            statusbarStatus.Content = $"Search Completed in {elapsedTime} Seconds";
            gridSearchResults.ItemsSource = searchresults;
            layoutSearchInfo.IsActive = true;
        }
    }
    public static class IPAddressExtensions
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }
        public static IPAddress CidrToNetmask(this int cidr)
        {
            String longmask = "";
            int zeroes = 32 - cidr;
            String dottednotation = "";

            //Add 1's
            for (int i = 0; i < cidr; i++)
            {
                longmask += "1";
            }

            //Add 0's
            for (int j = 0; j < zeroes; j++)
            {
                longmask += "0";
            }

            //Convert to netmask            
            for (int k = 0; k < 4; k++)
            {
                String section = "";
                int lastrange = (k * 8);
                section = longmask.Substring(lastrange, 8);
                dottednotation += Convert.ToInt32(section, 2);
                if (k < 3)
                {
                    dottednotation += ".";
                }
            }

            //Convert string to ip address
            IPAddress netmask = IPAddress.Parse(dottednotation);
            return netmask;
        }
    }
}
