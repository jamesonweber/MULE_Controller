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

using FFmpegInterop;
using Windows.Foundation.Collections;




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
        private GamepadReading gamepadDelta;
        private double deadZoneNeg = -0.1;
        private double deadZonePos = 0.1;

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

                        if(input.Equals(gamepadDelta))
                        {
                            return;
                        }

                        inputString = gamepad_packet_generator(input, cntrCounter);

                        // Gets the size of UTF-8 string.
                        writer.MeasureString(inputString);
                        // Write a string value to the output stream.
                        writer.WriteString(inputString);

                        gamepadDelta = input;
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
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                cntrCounter++;
            }
        }

        /* method to generate the controls packet sent to the Central Program */
        private String gamepad_packet_generator(GamepadReading input, long packetNumber)
        {
            String packetMeta = "CNTR|" + packetNumber.ToString() + "|" + input.Timestamp + "|";
            String packetEnd = "END|";
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
            /*
            if (!(input.RightThumbstickX <= deadZonePos && input.RightThumbstickX >= deadZoneNeg) )
            {
                returnString +=  "RightThumbstickX," + input.RightThumbstickX + "|";
            }
            else
            {
                returnString += "RightThumbstickX," + 0.0 + "|";
            }
            if (!(input.RightThumbstickY <= deadZonePos && input.RightThumbstickY >= deadZoneNeg))
            {
                returnString += "RightThumbstickY," + input.RightThumbstickY + "|";
            }
            else
            {
                returnString += "RightThumbstickY," + 0.0 + "|";
            }
            */

            returnString += getServos(input.RightThumbstickX, input.RightThumbstickY);

            if (!(input.LeftThumbstickX <= deadZonePos && input.LeftThumbstickX >= deadZoneNeg))
            {
                returnString += "LeftThumbstickX," + input.LeftThumbstickX + "|";
            }
            if (!(input.LeftThumbstickY <= deadZonePos && input.LeftThumbstickY >= deadZoneNeg))
            {
                returnString += "LeftThumbstickY," + input.LeftThumbstickY + "|";
            }

            return packetMeta + returnString + packetEnd;
        }

        private String getServos(double x, double y)
        {
            String plot = "";
            double piDivFour = 0.785398;
            double piDivTwo = 1.570796;
            double rightServo = 0.0;
            double leftServo = 0.0;

            //rightServo = Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivFour - Math.Atan(y / x)));
            //leftServo = Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivFour - Math.Atan(y / x)));

            if ((x > 0) && (y > 0) && (y > x)) // 45-90
            {
                rightServo = Math.Sqrt((x * x) + (y * y)) * (Math.Sin(Math.Atan(y / x) - piDivFour));
                leftServo = Math.Sqrt((x * x) + (y * y)) * (Math.Cos(Math.Atan(y / x) - piDivFour));

            }
            else if ((x > 0) && (y > 0) && (y < x)) //0-45
            {
                rightServo = -  Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivFour - Math.Atan(y / x)));
                leftServo = Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivFour - Math.Atan(y / x)));
            }
            else if ((x < 0) && (y > 0) && (y > Math.Abs(x))) //90-135
            {
                leftServo = Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivFour + piDivTwo - Math.Atan(y / x)));
                rightServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivFour + piDivTwo - Math.Atan(y / x)));
            }
            else if ((x < 0) && (y > 0) && (y < Math.Abs(x))) //135-180
            {
                rightServo = Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivFour - Math.Atan(y / x)));
                leftServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivFour - Math.Atan(y / x)));
            }
            else if ((x > 0) && (y < 0) && (Math.Abs(y) < x)) //315-360
            {
                rightServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivTwo + Math.Atan(y / x)));
                leftServo = Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivTwo + Math.Atan(y / x)));
            }
            else if ((x > 0) && (y < 0) && (Math.Abs(y) > x)) //270-315
            {
                rightServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivFour + piDivTwo - Math.Atan(y / x)));
                leftServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivFour + piDivTwo - Math.Atan(y / x)));
            }
            else if ((x < 0) && (y < 0) && (Math.Abs(y) > Math.Abs(x))) //225-270
            {
                rightServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Sin(Math.Atan(y / x) - piDivFour));
                leftServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Cos(Math.Atan(y / x) - piDivFour));
            }
            else if ((x < 0) && (y < 0) && (Math.Abs(y) < Math.Abs(x))) //180-225
            {
                rightServo = Math.Sqrt((x * x) + (y * y)) * (Math.Sin(piDivFour - Math.Atan(y / x)));
                leftServo = - Math.Sqrt((x * x) + (y * y)) * (Math.Cos(piDivFour - Math.Atan(y / x)));
            }

            plot = "ServoL," + leftServo + "|ServoR," + rightServo + "|"; 

            return plot;
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
                           controllerStatusTextBlock.Text = "XBOX Controller Status: Disconnected";
                      });
        }
        private async void gamepad_Added(object sender, Gamepad e)
        {
            gamepad = e;
            gamepadDelta = e.GetCurrentReading();

            await Dispatcher.RunAsync(
                  CoreDispatcherPriority.Normal, () =>
                  {
                       controllerStatusTextBlock.Text = "XBOX Controller Status: Connected";
                   });
        }


    }
}

