using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Data;
//for observableclass
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

namespace Cosmic_Rays
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            //calls function that determines if user has internet connection and checks if false
            if (!hasInterConnection())
            {
                //error message that user should check his/her internet connection
                MessageBox.Show("Het lijkt erop dat U geen internetverbinding heeft. Kijk uw verbinding na en probeer het nog een keer");
            }
            // gets a list of stations from the function
            ObservableCollection<Station> stationListSubRow = new StationList().initStations();
            // converts the list to a listcollectionview to add groupdescription
            ListCollectionView collection = new ListCollectionView(stationListSubRow);
            // adds the groupdescription to the listview
            collection.GroupDescriptions.Add(new PropertyGroupDescription("cluster"));
            // binds datagrid to listview collection
            GlobalStationList.GlobalStations = collection;

        }

        public static class GlobalStationList
        {
            public static ListCollectionView GlobalStations { get; set; }
        }


        public class StationList
        {
            public ObservableCollection<Station> initStations()
            {
                // declares webclient
                using (var webClient = new System.Net.WebClient())
                {
                    // gets json file
                    string clusterjson = webClient.DownloadString("http://data.hisparc.nl/api/subclusters/");
                    //converts .json to list of objects of class Station
                    ObservableCollection<Station> clusterlist = JsonConvert.DeserializeObject<ObservableCollection<Station>>(clusterjson);
                    // prepares empty observablecollection for the stations
                    ObservableCollection<Station> list = new ObservableCollection<Station>();
                    // loops thru every cluster in clusterlist
                    foreach (var item in clusterlist)
                    {
                        // downloads the stations from the cluster to raw json
                        string json = webClient.DownloadString("http://data.hisparc.nl/api/subclusters/" + item.stationID);
                        //converts json to observablecollection
                        ObservableCollection<Station> clusterliststation = JsonConvert.DeserializeObject<ObservableCollection<Station>>(json);
                        // loops through all station to add the cluster name to cluster variable in every station and add it to the stationlist
                        foreach (var i in clusterliststation)
                        {
                            i.cluster = item.stationName;
                            list.Add(i);
                        }
                    }
                    // returns the stationlist to the person who called the function
                    return list;
                }
            }
        }

        //defines object named stations
        public class Station
        {
            //defines string station property 'StationID' and binds it to the Jsonproperty number (from HiSparc Json)
            [JsonProperty("number")]
            public string stationID { get; set; }

            //defines string station property 'stationName' and binds it to the Jsonproperty name (from HiSparc Json)
            [JsonProperty("name")]
            public string stationName { get; set; }

            //defines boolean station property 'activeStation'
            public bool activeStation { get; set; }

            //defines boolean station property 'cluster'
            public string cluster { get; set; }

            //defines boolean station property 'selectedbyuser'
            public bool selectedByUser { get; set; }
        }


        
        //is called when user presses tab button
        private void button_Click(object sender, RoutedEventArgs e)
        {
            //defines ctrl as the button that is pressed
            Control ctrl = ((Control)sender);
            //changes the showed tab
            TabFrame.Source = new Uri("tabs/"+ctrl.Name+".xaml", UriKind.Relative);
            //loops thru all tab buttons
            foreach (Control item in TabButtonGrid.Children)
            {
                //sets all buttons to unselected color
                item.Background = new SolidColorBrush(Color.FromArgb(255, 110, 110, 110));
            }
            //changes the button of selected tab to the selected color
            ctrl.Background = new SolidColorBrush(Color.FromArgb(255, 149, 149, 149));
        }

        //function to check if user has internet connection
        public bool hasInterConnection()
        {
            InitializeComponent();
            try
            {
                //initializes ping function
                Ping myPing = new Ping();
                //determines host to ping
                String host = "google.com";
                //determines byte buffer for ping
                byte[] buffer = new byte[32];
                //determines timeout time for ping function
                int timeout = 1000;
                //loads other standard pingoptions
                PingOptions pingOptions = new PingOptions();
                //pings the host
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                //checks if ping was succesfull
                return (reply.Status == IPStatus.Success);
            }
            //checks if an error occured in above ping proces
            catch (Exception)
            {
                //if error occured reply that user has no internet access
                return false;
            }
        }
    }
}
