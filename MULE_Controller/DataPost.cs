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


    class DataPost
    {

        String description;

        private String dataType;
        private float sem;
        private String value;
        private float longitude;
        private float latitude;
        private float northings;
        private float eastings;
        private float depth;
        private String datetime;
        private String[] detailsValues = new String[10];

        public DataPost(int sensor, String description, String dataType, float sem, String value, float longitude, float latitude,
            float northings, float eastings, float depth, String datetime, String[] detailsValues)
        {
            this.description = description;
            this.dataType = dataType;
            this.sem = sem;
            this.value = value;
            this.longitude = longitude;
            this.latitude = latitude;
            this.northings = northings;
            this.eastings = eastings;
            this.depth = depth;
            this.datetime = datetime;
            this.detailsValues = detailsValues;
        }



    }
}
