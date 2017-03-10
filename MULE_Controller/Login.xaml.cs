using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
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

namespace MULE_Controller
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            if ((App.Current as App).isLoggedIn == true)
            {
                this.Frame.Navigate(typeof(OnlinePoster));
            }
            else
            {
                this.Frame.Navigate(typeof(Login));
            }
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string dbpass = null;
            errorText.Text = "Signing In...";
            muleServiceReference.Service1Client msr = new muleServiceReference.Service1Client();
            dbpass = await msr.checkLoginAsync(usernameText.Text);
            if(dbpass.Equals(sha256_hash(passwordText.Password)))
            {
                (App.Current as App).isLoggedIn = true;
                (App.Current as App).userName = usernameText.Text;
                errorText.Text = "";
                this.Frame.Navigate(typeof(OnlinePoster));
            }
            else
            {
                errorText.Text = "Invalid username or password.";
            }
            
        }

        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
