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
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var base_url = "http://data.hisparc.nl/data/network/coincidences/?";
            var startDate = BeginDate.SelectedDate;
            var endDate = EndDate.SelectedDate;
            var n = StationCount.Text;
            var cluster = "Aarhus";
            var stations = "None";
            string encodec_string = WebUtility.UrlEncode("cluster=Aarhus, 'stations': None, 'start': (2013, 9, 2), 'end': (2013, 9, 3), 'n': 2");
            var url = ("cluster=" + cluster + "&stations=" + stations + "&start=" + startDate.Value.Year + "-" + startDate.Value.Month + "-" + startDate.Value.Day + "&end=" + endDate.Value.Year + "-" + endDate.Value.Month + "-" + endDate.Value.Day + "&n=2");
            WebClient wc = new WebClient();
            var data = wc.DownloadString(base_url + url);
            coincidenties.Text = data;
            tempbox.Text = url;
        }            
    }     
}

