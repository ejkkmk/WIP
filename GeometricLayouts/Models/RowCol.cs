using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeometricLayouts.Models
{
    public class RowCol
    {
        //Status value to send to back to caller. Will be 0 if coordinates could successfully be determined, otherwise nonzero
        public int Status { get; set; }

        public string Row { get; set; }
        public string Col { get; set; }
    }
}