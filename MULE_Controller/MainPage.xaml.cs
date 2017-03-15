using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.Gaming.Input;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Text;
using System.IO;
using System.Collections.Generic;
using VLC;





// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/*******************************
* References
* ******************************
* https://sandervandevelde.wordpress.com/2016/03/18/control-your-arduino-rover-using-firmata-and-xbox-one-controller/
* http://donatas.xyz/streamsocket-tcpip-client.html
* http://stackoverflow.com/questions/39401969/uwp-navigation-pane-issues
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
        //String dns = "169.254.49.188"; 

        private String dns = "localhost";
        private String port = "8888";

        private String dataport = "8889";

        private StreamSocket socket;
        private DataWriter writer;

        private StreamSocket datasocket;

        private Gamepad gamepad = null;
        private GamepadReading gamepadDelta;
        private double deadZoneNeg = -0.1;
        private double deadZonePos = 0.1;

        private DataPost s1 = new DataPost();
        private DataPost s2 = new DataPost();

        public MainPage()
        {
            this.InitializeComponent();

            if ((App.Current as App).mp == null)
            {
                (App.Current as App).mp = new VLC.MediaElement();
                (App.Current as App).mp.Source = new Uri("http://169.254.49.188:8090");
                (App.Current as App).mp.AreTransportControlsEnabled = true;
                vlcGrid.Children.Add((App.Current as App).mp);
                (App.Current as App).mp.Play();
            }  
            else
            {
                (App.Current as App).mp = new VLC.MediaElement();
                (App.Current as App).mp.Source = new Uri("http://169.254.49.188:8090");
                (App.Current as App).mp.AreTransportControlsEnabled = true;
                vlcGrid.Children.Add((App.Current as App).mp);
                (App.Current as App).mp.Play();
            }    

            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
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
                //await central_program_Connect(dns, port);
            }

            if (datasocket == null)
            {
                await data_program_Connect(dns, dataport);
                collect_display_modules();
            }

            if (gamepad == null)
            {
                gamepad_Controls();
            }
        }

        /* Method to manage the recieving of module information from the data program */
        private async void collect_display_modules()
        {

            String packet = null;
            String initialModule;
            String[] splitPacket = null;
            String[] splitDetails;
            int[] splitDetailsInt = new int[10];
            String[] splitPos;
            float[] splitPosFloat = new float[3];

            int sensor;
            String serial;
            String dataType;
            String metaData;
            float sem;
            float sd;
            float avg;
            int[] detailsValues;
            float northings;
            float eastings;
            float depth;
            String datetime;

            while(packet == null)
            {
                packet = await data_program_read();                
            }
            
            splitPacket = packet.Split(new char[] { '|' });
            splitDetails = splitPacket[9].Split(new char[] { ',' });
            for(int i = 0; i<10; i++)
            {
                splitDetailsInt[i] = Convert.ToInt32(splitDetails[i]);
            }
            splitPos = splitPacket[10].Split(new char[] { ',' });
            for (int i = 0; i<3; i++)
            {
                splitPosFloat[i] = float.Parse(splitPos[i]);
            }


            initialModule = splitPacket[1]+splitPacket[3];

            sensor = 1;
            serial = splitPacket[3];
            dataType = splitPacket[5];
            metaData = splitPacket[2];
            sem = float.Parse(splitPacket[7]);
            sd = float.Parse(splitPacket[6]);
            avg = float.Parse(splitPacket[8]);
            detailsValues = splitDetailsInt;
            northings = splitPosFloat[0];
            eastings = splitPosFloat[1];
            depth = splitPosFloat[2];
            datetime = splitPacket[11];

            s1.setDataPost(sensor, serial, dataType, metaData, sem, sd, avg, detailsValues, 
                northings, eastings, depth, datetime);

            module1Display.Text = "Module 1: " + s1.metaData + " " + s1.avg + " " + s1.dataType;
            northingsDisplay.Text = "Northings: " + s1.northings;
            eastingsDisplay.Text = "Eastings: " + s1.eastings;
            depthDisplay.Text = "Depth: " + s1.depth + " M";

            while (true)
            {

                try { packet = await data_program_read();  }
                catch (Exception) { return; }
                
                splitPacket = packet.Split(new char[] { '|' });
                splitDetails = splitPacket[9].Split(new char[] { ',' });
                for (int i = 0; i < 10; i++)
                {
                    splitDetailsInt[i] = Convert.ToInt32(splitDetails[i]);
                }
                splitPos = splitPacket[10].Split(new char[] { ',' });
                for (int i = 0; i < 3; i++)
                {
                    splitPosFloat[i] = float.Parse(splitPos[i]);
                }
                initialModule = splitPacket[1] + splitPacket[3];

                sensor = 1;
                serial = splitPacket[3];
                dataType = splitPacket[5];
                metaData = splitPacket[2];
                sem = float.Parse(splitPacket[7]);
                sd = float.Parse(splitPacket[6]);
                avg = float.Parse(splitPacket[8]);
                detailsValues = splitDetailsInt;
                northings = splitPosFloat[0];
                eastings = splitPosFloat[1];
                depth = splitPosFloat[2];
                datetime = splitPacket[11];

                if(initialModule.Equals(splitPacket[1] + splitPacket[3]))
                {
                    lock(s1)
                    {
                        s1.setDataPost(sensor, serial, dataType, metaData, sem, sd, avg, detailsValues,
                            northings, eastings, depth, datetime);
                    }
                    
                    module1Display.Text = "Module 1: " + s1.metaData + " " + s1.avg + " " + s1.dataType;
                }
                else
                {
                    lock(s2)
                    {
                        s2.setDataPost(sensor, serial, dataType, metaData, sem, sd, avg, detailsValues,
                            northings, eastings, depth, datetime);
                    }
                    
                    module2Display.Text = "Module 1: " + s1.metaData + " " + s1.avg + " " + s1.dataType;
                }
                northingsDisplay.Text = "Northings: " + s1.northings;
                eastingsDisplay.Text = "Eastings: " + s1.eastings;
                depthDisplay.Text = "Depth: " + s1.depth + " M";

            }

        }

        /* Method to read data program packets */
        private async Task<String> data_program_read()
        {
            Stream streamIn = datasocket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(streamIn);
            String response = await reader.ReadLineAsync();
            return response;
        }

        /* Method for connecting to the data program of the MULE */
        private async Task data_program_Connect(String hostStr, String port)
        {
            datasocket = new StreamSocket();
            HostName host = new HostName(hostStr);
            try
            {
                // Connect to the server
                await datasocket.ConnectAsync(host, port);

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
            dataprogramStatusTextBlock.Text = "MULE Sensors Status: Connected";

            Stream streamOut = datasocket.OutputStream.AsStreamForWrite();
            StreamWriter writer = new StreamWriter(streamOut);
            string request = "handshake";
            await writer.WriteLineAsync(request);
            await writer.FlushAsync();

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

            returnString += getServos(input.RightThumbstickX, input.RightThumbstickY);            
            returnString += getRStick("LeftThumbstickX", input.LeftThumbstickX);
            returnString += getRStick("LeftThumbstickY", input.LeftThumbstickY);

            return packetMeta + returnString + packetEnd;
        }

        private String getRStick (String label, double x)
        {
            String plot = "";
            double defaultX = 0.0;

            if (!(x <= deadZonePos && x >= deadZoneNeg)) 
            {
                defaultX = x;
            }

            plot = label + "," + defaultX + "|";

            return plot;

        }

        // Method to calculate servo angles for controller
        private String getServos(double x, double y)
        {
            String plot = "";
            double piDivFour = -0.785398;
            double rightServo = 0.0;
            double leftServo = 0.0;

            if ((!(x <= deadZonePos && x >= deadZoneNeg))
                || (!(y <= deadZonePos && y >= deadZoneNeg)))
            {
                rightServo = (x * Math.Cos(piDivFour)) - (y * Math.Sin(piDivFour));
                leftServo = (x * Math.Sin(piDivFour)) + (y * Math.Cos(piDivFour));
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

        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            datasocket.Dispose();
            if ((App.Current as App).isLoggedIn==true)
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

        private void queueButton_Click(object sender, RoutedEventArgs e)
        {

            if (sensorCombo.SelectionBoxItem.ToString().Equals("Module 1"))
            {
                if (s1 == null) { return; }
                lock (s1)
                {
                    DataPost qs1 = new DataPost();
                    qs1.setDataPost(s1.sensor, s1.serial, s1.dataType, s1.metaData, s1.sem, s1.sd, s1.avg, s1.detailsValues, s1.northings, s1.eastings, s1.depth, s1.datetime);
                    qs1.description = descriptionText.Text;
                    (App.Current as App).dpList.Add(qs1);
                }
            }
            else if (sensorCombo.SelectionBoxItem.ToString().Equals("Module 2"))
            {
                if (s2 == null) { return; }
                lock (s2)
                {
                    DataPost qs2 = new DataPost();
                    qs2.setDataPost(s2.sensor, s2.serial, s2.dataType, s2.metaData, s2.sem, s2.sd, s2.avg, s2.detailsValues, s2.northings, s2.eastings, s2.depth, s2.datetime);
                    qs2.description = descriptionText.Text;
                    (App.Current as App).dpList.Add(s2);
                }
            }
        }
    }
}

