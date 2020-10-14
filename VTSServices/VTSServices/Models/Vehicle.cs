using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTSServices.Models
{
    public class Vehicle
    {
        public int VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineNumber { get; set; }
        public string Manufacturingyear { get; set; }

        public string Loadcarryingcapacity { get; set; }
        public string Makeofvehicle { get; set; }
        public string ModelNumber { get; set; }
        public string Bodytype { get; set; }
        public string Organisationname { get; set; }

        public string DeviceName { get; set; }
        public string UserName { get; set; }
    }
}