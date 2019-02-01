using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeometricLayouts.Models
{
    public class Coordinate
    {
        //Status value to send to back to caller. Will be 0 if coordinates could successfully be determined, otherwise nonzero
        public int Status { get; set; }    

        public int V1x { get; set; } 
        public int V1y { get; set; }

        public int V2x { get; set; }
        public int V2y { get; set; }

        public int V3x { get; set; }
        public int V3y { get; set; }
    }
}