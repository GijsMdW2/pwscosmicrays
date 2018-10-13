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
// add observable collections
using System.Collections.ObjectModel;
// add json usage
using Newtonsoft.Json;
// add url encode
using System.Net;


namespace Cosmic_Rays.tabs
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {

        public Page1()
        {
            InitializeComponent();
            stationGrid.ItemsSource = MainWindow.GlobalStationList.GlobalStations;
        }

        private void stationDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // calls update station function when filterdate is changed
            UpdateActiveStations();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string stationID = "";
            foreach (var item in stationGrid.Items.OfType<MainWindow.Station>())
            {
                // loops thru all items in activestation list
                if (item.selectedByUser == true) 
                    {
                    stationID = stationID + item.stationID + " ";
                    }
            }
            var base_url = "http://data.hisparc.nl/data/network/coincidences/?";
            var startDate = BeginDate.SelectedDate;
            var endDate = EndDate.SelectedDate;
            var n = StationCount.Text;
            var cluster = "Aarhus";
            var stations = "None";
            string encodec_string = WebUtility.UrlEncode("cluster=Aarhus, 'stations': None, 'start': (2013, 9, 2), 'end': (2013, 9, 3), 'n': 2");
            var url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "&n="+n);
            WebClient wc = new WebClient();
            var data = wc.DownloadString(base_url + url);
            coincidenties.Text = data;
            tempbox.Text = url;
        }

        public void UpdateActiveStations()
        {
            using (var webClient = new System.Net.WebClient())
            {
                // checks of set date is earlier then today
                if (stationDateFilter.SelectedDate < DateTime.Now)
                {
                    // makes it so that the variable dateTimeFilter always has a value
                    DateTime dateTimeFilter = stationDateFilter.SelectedDate ?? DateTime.Now;
                    // gets raw json data from the server
                    string json = webClient.DownloadString($"http://data.hisparc.nl/api/stations/data/" + dateTimeFilter.ToString("yyyy") + "/" + dateTimeFilter.ToString("MM") + "/" + dateTimeFilter.ToString("dd") + "/");
                    // converts json data to .net list
                    List<MainWindow.Station> stationsActive = JsonConvert.DeserializeObject<List<MainWindow.Station>>(json);
                    // loops thru all items in datagrid
                    foreach (var item in stationGrid.Items.OfType<MainWindow.Station>())
                    {
                        // loops thru all items in activestation list
                        for (int i = 0; i < stationsActive.Count; i++)
                        {
                            // if 2 names mach station is marked active and function will break
                            if (stationsActive[i].stationName == item.stationName)
                            {
                                item.activeStation = true;
                                break;
                            }
                        }
                    }
                }
                // if the date set by user is in the future (must revise when time travel is within reach)
                else
                {
                    // sets the flags from all rows to false
                    foreach (var item in stationGrid.Items.OfType<MainWindow.Station>())
                    {
                        item.activeStation = false;
                    }
                }
                stationGrid.Items.Refresh();
            }
        }
    }     
}

