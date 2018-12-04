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
using System.Threading;


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
            loadingpanelShow();
            // calls update station function when filterdate is changed
            UpdateActiveStations();
            loadingpanelHide();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            loadingpanelShow();
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
            //declares begintime string variable
            var beginTimeString = BeginTime.Text;
            //declares endtime string variable
            var endTimeString = EndTime.Text;
            //sets begintimestring to 0 if left empty
            if (beginTimeString == "") { beginTimeString = "0"; }
            //sets endtimestring to 0 if left empty
            if (endTimeString == "") { endTimeString = "0"; }
            //converts selected hours to 32bit signed integer
            var beginHour = Convert.ToInt32(beginTimeString);
            //converts selected hours to 32bit signed integer
            var endHour = Convert.ToInt32(endTimeString);
            //declares startdate that user has selected
            var startDate = BeginDate.SelectedDate;
            //checks if startdate is entered by user
            if (startDate == null)
            {
                //gives error message to enter startdate
                MessageBox.Show("Voer een begindatum in");
                return;
            }
            //adds selected hour to startdate
            startDate = startDate.Value.Add(new TimeSpan(beginHour, 0, 0));
            //declares enddate that user has selected
            var endDate = EndDate.SelectedDate;
            //checks if enddate has a value
            if (endDate == null)
            {
                //gives error message that the enddate has to be given
                MessageBox.Show("Voer een einddatum in");
                //quits program
                return;
            }
            //adds selected hour to enddate
            endDate = endDate.Value.Add(new TimeSpan(endHour, 0, 0));
            //checks if all characters for the N value are indeed numeric charactes
            if (!n.All(char.IsDigit))
            {
                //gives error message that there are non-numeric characters detected
                MessageBox.Show("Voer alleen getallen in voor de N waarde");
                //exits function
                return;
            }
            //checks if all characters for the begintime value are indeed numeric charactes
            if (!BeginTime.Text.All(char.IsDigit))
            {
                //gives error message that there are non-numeric characters detected
                MessageBox.Show("Voer alleen getallen in voor de het beginuur");
                //exits function
                return;
            }
            //checks if the entered beginhour is bigger then 23
            if (beginHour > 23)
            {
                //gives error message that entered hour should be smaller then 24
                MessageBox.Show("Het ingevulde beginuur moet kleiner zijn dan 24");
                //exits function
                return;
            }
            //checks if the entered endhour is bigger then 23
            if (endHour > 23)
            {
                //gives error message that entered hour should be smaller then 24
                MessageBox.Show("Het ingevulde einduur moet kleiner zijn dan 24");
                //exits function
                return;
            }
            //checks if all characters for the endtime value are indeed numeric charactes
            if (!EndTime.Text.All(char.IsDigit))
            {
                //gives error message that there are non-numeric characters detected
                MessageBox.Show("Voer alleen getallen in voor de het einduur");
                //exits function
                return;
            }
            //checks if N value is given
            if (n == "")
            {
                MessageBox.Show("Voer een waarde in voor N");
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
            //checks if there is only 1 char in begintimestring
            if (beginTimeString.Length ==1)
            {
                //adds a 0 in front of string if length = 1
                beginTimeString = "0" + beginTimeString;
            }
            //checks if there is only 1 char in endtimestring
            if (endTimeString.Length == 1)
            {
                //adds a 0 in front of string if length = 1
                endTimeString = "0" + endTimeString;
            }
            //declares variable URL
            var url = "";
            //checks if the hours in the selected time = 0 (else the server will answer with bad request)
            if (startDate.Value.Hour!=0)
            {
                //creates the final url to be used for data call
                url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "+" + beginTimeString + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "+" + endTimeString + "&n=" + n);
            }
            else
            {
                url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "&n=" + n);
            }
            // clears stations string data for next request
            stations = "";
            //declares lines variable
            int lines = 0;
            //creates a task that will run async so that UI won't freeze while downloading data
            await Task.Run(() =>
            {
                //initiates webclient
                WebClient wc = new WebClient();
                //gets the response from the server in an streamreader entity
                var data =  wc.OpenRead(base_url + url);
                //var data = await wc.OpenReadTaskAsync(base_url + url);
                using (StreamReader r = new StreamReader(data))
                {
                    //defines variable for line currently looked at by streamreader
                    string line;
                    //counts the lines from the server response (= the ammount of coincidences)
                    while ((line = r.ReadLine()) != null)
                    {
                        var tabsplitline = line.Split('\t');
                        if (tabsplitline[0] == lines.ToString())
                        {
                            lines++;
                        }
                    }
                }
                //removes a line because 1 line to many is counted
                lines = lines - 1;
                //recorrects to 0 if value becomes -1 because no document is received
                if (lines == -1)
                {
                    lines = 0;
                }

            });
            //sets anwswer in textbox
            coincidenties.Text = "Aantal coïncidenties: ";
            coincidenties.Inlines.Add(new Bold(new Run(lines.ToString())));
            tempbox.Text = base_url + url;
            loadingpanelHide();
        }

        //function for updating the list of active stations
        public async void UpdateActiveStations()
        {
            using (var webClient = new System.Net.WebClient())
            {
                // checks of set date is earlier then today
                if (stationDateFilter.SelectedDate < DateTime.Now)
                {
                    // makes it so that the variable dateTimeFilter always has a value
                    DateTime dateTimeFilter = stationDateFilter.SelectedDate ?? DateTime.Now;
                    // declares variable json that will hold raw json data from server
                    string json = "";
                    // gets raw json data from the server with an async task
                    await Task.Run(() =>
                    {
                        json = webClient.DownloadString($"http://data.hisparc.nl/api/stations/data/" + dateTimeFilter.ToString("yyyy") + "/" + dateTimeFilter.ToString("MM") + "/" + dateTimeFilter.ToString("dd") + "/");
                    });
                    
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

        private void infobutton_Click(object sender, RoutedEventArgs e)
        {
            if (infocollapsbutton.Content == ">")
            {
                infocolumn.Width = new GridLength(30);
                infocollapsbutton.Content = "<";
                Infoscrollviewer.Visibility = Visibility.Hidden;
            }
            else
            {
                infocolumn.Width = new GridLength(200);
                infocollapsbutton.Content = ">";
                Infoscrollviewer.Visibility = Visibility.Visible;
            }
        }

        private void loadingpanelHide()
        {
            LoadingPanel.Visibility = Visibility.Hidden;
        }
        private void loadingpanelShow()
        {
            LoadingPanel.Visibility = Visibility.Visible;
        }

    }     
}

