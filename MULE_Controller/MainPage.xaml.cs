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

using Windows.Gaming.Input;
using Windows.UI.Core;
using System.Threading.Tasks;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/*******************************
* References
* ******************************
* https://sandervandevelde.wordpress.com/2016/03/18/control-your-arduino-rover-using-firmata-and-xbox-one-controller/
* ******************************
* Documentation
* ******************************
* Author: Jameson Weber
* Date:   Nov 19 2016
* Desc:   Class for creating a sensor packet object. These are generic objects designed to hold data temporarily in a queue
*          awaiting a "data post" trigger. At that point 10 of these pakcets from the same sensor will be inserted into the
*          sensor details table
*          
* ******************************          
* Modifications
* ******************************
* Author: 
* Date:   
* Desc:   
* 
*/

namespace MULE_Controller
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private Gamepad gamepad = null;

        public MainPage()
        {
            this.InitializeComponent();
            
        }

        /* Method to deal with syncing the application with all external dependancies
         * This includes:
         * -Controller
         * -Sensors
         * -Video Feed
         */
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (gamepad == null)
            {
                gamepad_Controls();
            }
        }
        
        /* Method for dealing with button actions of connected controller */
        private async void gamepad_Controls()
        { 
            Gamepad.GamepadAdded += gamepad_Added;
            Gamepad.GamepadRemoved += gamepad_Removed;

            while (true)
            {
                await Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, () =>
                    { 
                        if (gamepad == null)
                        {
                            return;
                        }

                        var input = gamepad.GetCurrentReading();

                        controllerInputTextBlock.Text = input.RightThumbstickX.ToString();
                       
                    });
                await Task.Delay(TimeSpan.FromMilliseconds(5));
            }
        }


        /* Async methods to deal with when a controller is connected and disconnected while the application is running */
        private async void gamepad_Removed(object sender, Gamepad e)
        {
            gamepad = null;

            await Dispatcher.RunAsync(
                 CoreDispatcherPriority.Normal, () =>
                     {
                           controllerStatusTextBlock.Text = "Controller Status: Disconnected";
                      });
        }
        private async void gamepad_Added(object sender, Gamepad e)
        {
            gamepad = e;

            await Dispatcher.RunAsync(
                  CoreDispatcherPriority.Normal, () =>
                  {
                       controllerStatusTextBlock.Text = "Controller Status: Connected";
                   });
        }
    }
}

