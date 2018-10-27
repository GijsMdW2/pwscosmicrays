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
using System.IO;
using System.Text.RegularExpressions;


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
            //binds datagrid datasource to global data variable
            stationGrid.ItemsSource = MainWindow.GlobalStationList.GlobalStations;
        }

        private void stationDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // calls update station function when filterdate is changed
            UpdateActiveStations();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //creates list that will hold by user selected stations
            List<string> stationlist = new List<string>();
            //loops through every item in station datagrid
            foreach (var item in stationGrid.Items.OfType<MainWindow.Station>())
            {
                // checks if the item currently in interest is selected by the user
                if (item.selectedByUser == true) 
                    {
                    //adds the stationID to the stationlist
                    stationlist.Add(item.stationID);
                    }
            }
            //declares the ammount of stations that need to register an even for a coincidence to log
            var n = StationCount.Text;
            //declares startdate that user has selected
            var startDate = BeginDate.SelectedDate;
            //declares enddate that user has selected
            var endDate = EndDate.SelectedDate;
            //checks if all characters vor the N value are indeed numeric charactes
            if (!n.All(char.IsDigit))
            {
                //gives error message that there are non-numeric characters detected
                MessageBox.Show("Vul alleen getallen in voor de N waarde");
                //exits function
                return;
            }
            //checks if more stations are required for event than stations are selected
            if (Convert.ToInt32(n) > stationlist.Count)
            {
                //gives error message box
                MessageBox.Show("Er zijn te weinig stations geselecteerd of de N waarde moet worden verlaagd");
                //exits function
                return;
            }
            //checks if startdate is smaller than enddate
            if (startDate > endDate)
            {
                //gives error message that startdate is later than begindate
                MessageBox.Show("De geselecteerde begindatum moet eerder zijn dan de geselecteerde einddatum");
                //exits function
                return;
            }
            //checks if selected period is not in future
            if (endDate > DateTime.Now)
            {
                //gives error message that tiem period cannot be in the future
                MessageBox.Show("De geselecteerde periode kan niet in de toekomst zijn");
                //exits function
                return;
            }
            //initial URL code for first station item
            string stations = "%5B%27";
            //loops thru all the items in stationlist except the last one
            for (int i = 0; i < stationlist.Count()-1; i++)
            {
                //adds the stationID + url code for inbetween stationID's
                stations = stations + stationlist[i] + "%27%2C+%27";
            }
            //Adds the last stationID to the string and add the final URl code
            stations = stations + stationlist[stationlist.Count() - 1] + "%27%5D";
            //declares base URL for HiSparc API
            var base_url = "http://data.hisparc.nl/data/network/coincidences/?";
            //adds that we won't be using the cluster option from the API
            var cluster = "None";
            //string encodec_string = WebUtility.UrlEncode("cluster=Aarhus, 'stations': None, 'start': (2013, 9, 2), 'end': (2013, 9, 3), 'n': 2");
            //creates the final url to be used for data call
            var url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "&n="+n);
            // clears stations string data for next request
            stations = "";
            //initiates webclient
            WebClient wc = new WebClient();
            //creates the variable that will hold the ammount of lines of data received
            int lines = 0;
            //gets the response from the server in an streamreader entity
            using (StreamReader r = new StreamReader(WebRequest.Create(base_url + url).GetResponse().GetResponseStream()))
            {
                //counts the lines from the server response (= the ammount of coincidences)
                while (r.ReadLine() != null) { lines++; }
            }
            coincidenties.Text = "Aantal coïncidenties: " + lines.ToString();
            tempbox.Text = url;
        }

        //function for updating the list of active stations
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
                    //loops thru all objects to disable all active stations first (init reset)
                    foreach (var item in stationGrid.Items.OfType<MainWindow.Station>())
                    {
                        item.selectedByUser = false;
                    }
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

        //function to only allow numeric characters for textboxes which should only allow numeric characters (N value, hours etc.)
        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            //defines acceptable characters
            Regex reg = new Regex("[^0-9]");
            //checks if the inputted character falls under the acceptable characters and returns the character
            e.Handled = reg.IsMatch(e.Text);
        }
    }     
}

