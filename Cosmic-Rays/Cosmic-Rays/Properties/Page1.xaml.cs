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
using LiveCharts;
using LiveCharts.Wpf;

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
            //binds datagrid binding datasource to global data variable
            stationGrid.ItemsSource = MainWindow.GlobalStationList.GlobalStations;
            //creates a series collection that binds to the diagram, else it becomes bugged and wont redraw with new data
            SeriesCollection = new SeriesCollection
            {
                new LineSeries{ }
            };
            //datacontext for diagram
            DataContext = this;
            //xaxis description
            XaxisName = "Tijd (in uren)";
        }

        private async void stationDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            //shows the loadingscreen
            loadingpanelShow();
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
                        item.activeStation = false;
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
            //hides the loadingscreen when done
            loadingpanelHide();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //shows loading panel to notify to user program is working
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
                //hides loading panel
                loadingpanelHide();
                //quits program
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
                //hides loading panel
                loadingpanelHide();
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
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if all characters for the begintime value are indeed numeric charactes
            if (!BeginTime.Text.All(char.IsDigit))
            {
                //gives error message that there are non-numeric characters detected
                MessageBox.Show("Voer alleen getallen in voor de het beginuur");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if the entered beginhour is bigger then 23
            if (beginHour > 23)
            {
                //gives error message that entered hour should be smaller then 24
                MessageBox.Show("Het ingevulde beginuur moet kleiner zijn dan 24");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if the entered endhour is bigger then 23
            if (endHour > 23)
            {
                //gives error message that entered hour should be smaller then 24
                MessageBox.Show("Het ingevulde einduur moet kleiner zijn dan 24");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if all characters for the endtime value are indeed numeric charactes
            if (!EndTime.Text.All(char.IsDigit))
            {
                //gives error message that there are non-numeric characters detected
                MessageBox.Show("Voer alleen getallen in voor de het einduur");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if N value is given
            if (n == "")
            {
                MessageBox.Show("Voer een waarde in voor N");
                //hides loading panel
                loadingpanelHide();
                return;
            }
            //checks if more stations are required for event than stations are selected
            if (Convert.ToInt32(n) > stationlist.Count)
            {
                //gives error message box
                MessageBox.Show("Er zijn te weinig stations geselecteerd of de N waarde moet worden verlaagd");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if startdate is smaller than enddate
            if (startDate > endDate)
            {
                //gives error message that startdate is later than begindate
                MessageBox.Show("De geselecteerde begindatum moet eerder zijn dan de geselecteerde einddatum");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //checks if selected period is not in future
            if (endDate > DateTime.Now)
            {
                //gives error message that tiem period cannot be in the future
                MessageBox.Show("De geselecteerde periode kan niet in de toekomst zijn");
                //hides loading panel
                loadingpanelHide();
                //exits function
                return;
            }
            //initial URL code for first station item
            string stations = "%5B%27";
            //loops thru all the items in stationlist except the last one
            for (int i = 0; i < stationlist.Count() - 1; i++)
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
            if (beginTimeString.Length == 1)
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
            if (startDate.Value.Hour != 0)
            {
                //creates the final url to be used for data call if hours are used
                url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "+" + beginTimeString + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "+" + endTimeString + "&n=" + n);
            }
            else
                //creates the final url if hours aren't used for datacall
            {
                url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "&n=" + n);
            }
            // clears stations string data for next request
            stations = "";
            //declares coincidences variable
            int coincidences = 0;
            //creates an array that will hold the timestamps for the coincidences
            List<DateTime> dates = new List<DateTime>();
            //creates a task that will run async so that UI won't freeze while downloading data
            await Task.Run(() =>
            {
                //initiates webclient
                WebClient wc = new WebClient();
                //gets the response from the server in an streamreader entity
                var data = wc.OpenRead(base_url + url);
                //var data = await wc.OpenReadTaskAsync(base_url + url);
                using (StreamReader r = new StreamReader(data))
                {
                    //defines variable for line currently looked at by streamreader
                    string line;
                    //counts the lines from the server response (= the ammount of coincidences)
                    while ((line = r.ReadLine()) != null)
                    {
                        //creates an array of strings that are seperated by tabs
                        var tabsplitline = line.Split('\t');
                        //checks if the first tab indicates a new coincidence
                        if (tabsplitline[0] == coincidences.ToString())
                        {
                            //adds a 1 to the ammounnt of coincidences counted
                            coincidences++;
                            //adds the timestamp of the current coincidence to an array of timestamps
                            dates.Add(DateTime.Parse(tabsplitline[2] + " " + tabsplitline[3]));
                        }
                    }
                }
            });
            //declares variable that will hold the values for the chart
            var values = new ChartValues<double>();
            DateTime nonNullStartdate = startDate ?? DateTime.Now;
            TimeSpan period = endDate.Value.Subtract(nonNullStartdate);
            /*if (period.TotalHours < 49)
            {
                XaxisName = "Tijd (in 10 minuten)";
                for (int i = 0; i < (period.TotalHours * 6); i++)
                {
                    values.Add(0);
                    foreach (var item in dates)
                    {
                        if (Math.Round(item.Subtract(nonNullStartdate).TotalHours, 1) == i)
                        {
                            values[i] = values[i] + 1;
                        }
                    }
                }
                SeriesCollection[0] = (new LineSeries
                {
                    Title = "Coincidenties",
                    Values = values
                });
            }
            else*/
            //{
                XaxisName = "Tijd (in uren)";
                for (int i = 0; i < period.TotalHours; i++)
                {
                    values.Add(0);
                    foreach (var item in dates)
                    {
                        if (Math.Round(item.Subtract(nonNullStartdate).TotalHours) == i)
                        {
                            values[i] = values[i] + 1;
                        }
                    }
                }
                SeriesCollection[0] = (new LineSeries
                {
                    Title = "Coincidenties",
                    Values = values
                });
            //}
            
            //sets anwswer in textbox
            coincidenties.Text = "Aantal coïncidenties: ";
            //boldens the answer
            coincidenties.Inlines.Add(new Bold(new Run(coincidences.ToString())));
            //hides the loading panel
            loadingpanelHide();
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
            if (infocollapsbutton.Content.ToString() == ">")
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

        public SeriesCollection SeriesCollection { get; set; }
        public string XaxisName { get; set; }
    }     
}

