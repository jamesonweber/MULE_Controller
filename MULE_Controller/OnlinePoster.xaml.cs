using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

//Reference
//https://social.msdn.microsoft.com/Forums/vstudio/en-US/531f8bc7-4d62-464a-8180-beddaa46cb7d/programatically-adding-items-to-stackpanel?forum=wpf
//http://stackoverflow.com/questions/5611658/change-margin-programmatically-in-wpf-c-sharp

namespace MULE_Controller
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OnlinePoster : Page
    {

        ComboBox groupCombo;

        public OnlinePoster()
        {
            this.InitializeComponent();
            setUpUI();
        }

        private async void setUpUI()
        {
            if ((App.Current as App).dpList.Count() != 0)
            {
                Button btn = new Button();
                btn.Content = "Upload";
                btn.Click += upload_Click;
                uidpList.Children.Add(btn);

                TextBlock gtitle = new TextBlock();
                gtitle.FontWeight = FontWeights.Bold;
                gtitle.Padding = new Thickness(0, 25, 0, 0);
                gtitle.Text = "Select a group to add posts to";
                uidpList.Children.Add(gtitle);

                await setGroupCombo();
            }
            else if (!(App.Current as App).uploaded)
            {
                TextBlock title = new TextBlock();
                title.FontWeight = FontWeights.Bold;
                title.Text = "No data has been queued to be added at this time, posts can be queued from the Home page.";
                uidpList.Children.Add(title);
            }
            else if ((App.Current as App).uploaded)
            {
                TextBlock title = new TextBlock();
                title.FontWeight = FontWeights.Bold;
                title.Text = "Upload Successfull! Queue some more data from the Home page.";
                uidpList.Children.Add(title);
            }
            foreach (DataPost dp in (App.Current as App).dpList)
            {
                TextBlock title = new TextBlock();
                title.FontWeight = FontWeights.Bold;
                title.Padding = new Thickness(0, 25, 0, 0);
                title.Text = "Module " + dp.sensor;
                uidpList.Children.Add(title);
                TextBlock details = new TextBlock();
                details.Text = "Content: " + dp.metaData + " " + dp.avg + " " + dp.dataType;
                uidpList.Children.Add(details);
                TextBlock location = new TextBlock();
                location.Text = "Location: " + dp.northings + "N, " + dp.eastings + "E, " + dp.depth + "M";
                uidpList.Children.Add(location);
                if (!dp.description.Equals(""))
                {
                    TextBlock desc = new TextBlock();
                    desc.Text = "Description: " + dp.description;
                    uidpList.Children.Add(desc);
                }
            }
        }


        private async Task setGroupCombo()
        {
            muleServiceReference.Service1Client msr = new muleServiceReference.Service1Client();
            var groups = await msr.getGroupsAsync((App.Current as App).userName);

            groupCombo = new ComboBox();
            groupCombo.MinWidth = 400;

            foreach(var g in groups)
            {
                groupCombo.Items.Add(g);
            }

            uidpList.Children.Add(groupCombo);
        }

        private async void upload_Click(object sender, RoutedEventArgs e)
        {
            String uname = (App.Current as App).userName;
            String group = groupCombo.SelectionBoxItem.ToString();
            muleServiceReference.Service1Client msr = new muleServiceReference.Service1Client();
            foreach(var dp in (App.Current as App).dpList)
            {
                bool result = await msr.uploadPostsAsync(new muleServiceReference.DataPost { sensor = dp.sensor, serial = dp.serial, user_name = uname, group_name = group,
                    dataType = dp.dataType, metaData = dp.metaData, sem = dp.sem, sd = dp.sd, avg = dp.avg, detailsValues = null,
                            northings = dp.northings, eastings = dp.eastings, depth = dp.depth, datetime = dp.datetime });
            }

            (App.Current as App).dpList = null;
            (App.Current as App).dpList = new List<DataPost>();
            (App.Current as App).uploaded = true;
            this.Frame.Navigate(typeof(OnlinePoster));
        }

        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((App.Current as App).isLoggedIn == true)
            {
                this.Frame.Navigate(typeof(OnlinePoster));
            }
            else {
                this.Frame.Navigate(typeof(Login));
            }
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
