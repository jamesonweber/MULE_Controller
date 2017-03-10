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
 * Desc:   Class for creating a data post object. The details value will be populated from the SensorPacket values in the queue.
 *          The rest will be populated from the most resent sensor packet from the queue to be inserted into the sensor table
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


    public class DataPost
    {
        public int sensor { get; set; }
        public String description { get; set; }

        public String group_name { get; set; }
        public String user_name { get; set; }

        public String serial { get; set; }
        public String dataType { get; set; }
        public String metaData { get; set; }
        public float sem { get; set; }
        public float sd { get; set; }
        public float avg { get; set; }
        public int[] detailsValues { get; set; }
        public float northings { get; set; }
        public float eastings { get; set; }
        public float depth { get; set; }
        public String datetime { get; set; }

        

        public void setDataPost(int sensor, String serial, String dataType, String metaData, float sem, float sd, float avg, int[] detailsValues,
            float northings, float eastings, float depth, String datetime)
        {
            this.sensor = sensor;
            this.serial = serial;
            this.dataType = dataType;
            this.metaData = metaData;
            this.sem = sem;
            this.sd = sd;
            this.avg = avg;
            this.detailsValues = detailsValues;
            this.northings = northings;
            this.eastings = eastings;
            this.depth = depth;
            this.datetime = datetime;
        }


    }
}
