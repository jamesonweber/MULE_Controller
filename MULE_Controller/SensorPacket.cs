using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*******************************
* References
* ******************************
* 
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
 * Modifications:
 * ******************************
 * Author: 
 * Date:   
 * Desc:   
 * 
 */

namespace MULE_Controller
{
    class SensorPacket
    {
        private int sensor;
        private int counter;
        private String dataType;
        private float sem;
        private String value;
        private float longitude;
        private float latitude;
        private float northings;
        private float eastings;
        private float depth;
        private String datetime;

        public SensorPacket(int sensor, int counter, String dataType, float sem, String value, float longitude, float latitude, 
            float northings, float eastings, float depth, String datetime)
        {
            this.sensor = sensor;
            this.counter = counter;
            this.dataType = dataType;
            this.sem = sem;
            this.value = value;
            this.longitude = longitude;
            this.latitude = latitude;
            this.northings = northings;
            this.eastings = eastings;
            this.depth = depth;
            this.datetime = datetime;
        }



    }
}
