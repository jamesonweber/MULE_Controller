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


        public MainPage()
        {
            this.InitializeComponent();

        }


    }
}
