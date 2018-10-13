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

namespace Cosmic_Rays
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            // gets a list of stations from the function
            ObservableCollection<Station> stationListSubRow = new StationList().loadSations();
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

        private void stationDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // calls update station function when filterdate is changed
            
        }


        public class StationList
        {
            public ObservableCollection<Station> loadSations()
            {
                return initStations();
            }

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

        public class Station
        {
            [JsonProperty("number")]
            public string stationID { get; set; }

            [JsonProperty("name")]
            public string stationName { get; set; }

            public bool activeStation { get; set; }

            public string cluster { get; set; }

            public bool selectedByUser { get; set; }
        }


        

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Control ctrl = ((Control)sender);
            TabFrame.Source = new Uri("tabs/"+ctrl.Name+".xaml", UriKind.Relative);
            foreach (Control item in TabButtonGrid.Children)
            {
                item.Background = new SolidColorBrush(Color.FromArgb(255, 110, 110, 110));
            }
            ctrl.Background = new SolidColorBrush(Color.FromArgb(255, 149, 149, 149));
        }
    }
}
