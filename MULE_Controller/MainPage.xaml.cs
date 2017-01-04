﻿using System;
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
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;


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

        private StreamSocket socket;
        private DataWriter writer;
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
        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {

            if (socket == null)
            {
                await central_program_Connect("localhost", "8888");
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
                        controllerInputTextBlock.Text = input.Buttons.ToString();
                        inputString = input.Buttons.ToString();
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

