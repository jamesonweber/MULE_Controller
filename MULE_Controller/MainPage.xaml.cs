using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.Gaming.Input;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Media.Core;
using Windows.Storage;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/*******************************
* References
* ******************************
* https://sandervandevelde.wordpress.com/2016/03/18/control-your-arduino-rover-using-firmata-and-xbox-one-controller/
* http://donatas.xyz/streamsocket-tcpip-client.html
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
        //String dns = "localhost";
        String dns = "169.254.49.188"; 
        String port = "8888"; 

        private StreamSocket socket;
        private DataWriter writer;
        private Gamepad gamepad = null;
        private double deadZoneNeg = -0.1;
        private double deadZonePos = 0.1;

        public MainPage()
        {
            this.InitializeComponent();
            //System.Uri manifestUri = new Uri("http://169.254.49.188:8080/");
            //System.Uri manifestUri = new Uri("ms-appx:///Assets/ltw.mp4", UriKind.Absolute);.
            //videoPlayer.Source = new Uri("http://rdmedia.bbc.co.uk/dash/ondemand/bbb/2/client_manifest-common_init.mpd");
            videoPlayer.Source = new Uri("http://169.254.49.188:8080/");
            videoPlayer.Play();
        }

        /* Method to deal with syncing the application with all external dependancies
         * This includes:
         * -Controller
         * -Sensors
         * -Video Feed
         */
        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {


            if (socket == null)
            {
                await central_program_Connect(dns, port);
            }

            if (gamepad == null)
            {
                gamepad_Controls();
            }
        }
        

        /* Method for connecting to the central program of the MULE */
        private async Task central_program_Connect(String hostStr, String port)
        {
            socket = new StreamSocket();
            HostName host = new HostName(hostStr);
            try
            {
                // Connect to the server
                await socket.ConnectAsync(host, port);

            }
            catch (Exception exception)
            {
                switch (SocketError.GetStatus(exception.HResult))
                {
                    case SocketErrorStatus.HostNotFound:
                        // Handle HostNotFound Error
                        throw;
                    default:
                        // If this is an unknown status it means that the error is fatal and retry will likely fail.
                        throw;
                }
            }
            centralprogramStatusTextBlock.Text = "MULE Controls Status: Connected";
            

            writer = new DataWriter(socket.OutputStream);
            // Set the Unicode character encoding for the output stream
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            // Specify the byte order of a stream.
            writer.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;      
        }

        
        /* Method for dealing with button actions of connected controller */
        private async void gamepad_Controls()
        { 
            Gamepad.GamepadAdded += gamepad_Added;
            Gamepad.GamepadRemoved += gamepad_Removed;

            long cntrCounter = 0;

            while (true)
            {
                await Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, () =>
                    { 
                        if (gamepad == null)
                        {
                            return;
                        }

                        String inputString;
                        GamepadReading input = gamepad.GetCurrentReading();
                        inputString = gamepad_packet_generator(input, cntrCounter);

                        // Gets the size of UTF-8 string.
                        writer.MeasureString(inputString);
                        // Write a string value to the output stream.
                        writer.WriteString(inputString);

                    });
                try
                {
                    await writer.StoreAsync();
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            socket = null;
                            writer = null;
                            centralprogramStatusTextBlock.Text = "MULE Controls Status: Disconnected";
                            return;
                        default:
                            socket = null;
                            writer = null;
                            centralprogramStatusTextBlock.Text = "MULE Controls Status: Disconnected";
                            return;
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(5));
                cntrCounter++;
            }
        }

        /* method to generate the controls packet sent to the Central Program */
        private String gamepad_packet_generator(GamepadReading input, long packetNumber)
        {
            String packetMeta = "CNTR|" + packetNumber.ToString() + "|" + input.Timestamp + "|";
            String buttonString;
            String[] parsedButtons = null;
            String returnString = "";

            buttonString = input.Buttons.ToString();
            parsedButtons = buttonString.Split(new char[] { ',' });
            foreach(String s in parsedButtons)
            {
                returnString += s.Trim() + "|" ;
            }

            if(input.RightTrigger > 0.0)
            {
                returnString += getRightTrigger(input, parsedButtons, "RightTrigger,");
            }
            if (input.LeftTrigger > 0.0)
            {
                returnString += getLeftTrigger(input, parsedButtons, "LeftTrigger,");
            }

            if (!(input.RightThumbstickX <= deadZonePos && input.RightThumbstickX >= deadZoneNeg) )
            {
                returnString +=  "RightThumbstickX," + input.RightThumbstickX + "|";
            }
            if (!(input.RightThumbstickY <= deadZonePos && input.RightThumbstickY >= deadZoneNeg))
            {
                returnString += "RightThumbstickY," + input.RightThumbstickX + "|";
            }

            if (!(input.LeftThumbstickX <= deadZonePos && input.LeftThumbstickX >= deadZoneNeg))
            {
                returnString += "LeftThumbstickX," + input.LeftThumbstickX + "|";
            }
            if (!(input.LeftThumbstickY <= deadZonePos && input.LeftThumbstickY >= deadZoneNeg))
            {
                returnString += "LeftThumbstickY," + input.LeftThumbstickY + "|";
            }

            return packetMeta + returnString;

        }
        
        /* methods to append trigger information to the controls packet */
        private String getRightTrigger(GamepadReading input, String[] parsedButtons, String label)
        {
            foreach(String s in parsedButtons)
            {
                if(s.Trim().Equals("RightShoulder"))
                {
                    return "";
                }
            }
            return label + input.RightTrigger.ToString() + "|";
        }
        private String getLeftTrigger(GamepadReading input, String[] parsedButtons, String label)
        {
            foreach (String s in parsedButtons)
            {
                if (s.Trim().Equals("LeftShoulder"))
                {
                    return "";
                }
            }
            return label + input.LeftTrigger.ToString() + "|";
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

