using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

namespace MULE_Controller
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OnlinePoster : Page
    {
        public OnlinePoster()
        {
            this.InitializeComponent();

            if((App.Current as App).dpList.Count() != 0)
            {
                Button btn = new Button();
                btn.Content = "Upload";
                uidpList.Children.Add(btn);
            }
            foreach(DataPost dp in (App.Current as App).dpList)
            {
                TextBlock title = new TextBlock();
                title.Text = "Module " + dp.sensor;
                uidpList.Children.Add(title);
                TextBlock avg = new TextBlock();
                title.Text = "Average:  " + dp.avg;
                uidpList.Children.Add(avg);

            }
            
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
