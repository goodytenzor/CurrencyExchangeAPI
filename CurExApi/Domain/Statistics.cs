using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurExApi.Domain
{
    public class Statistics
    {
        public double Min { get; set; }

        public DateTime MinDate { get; set; }

        public double Max { get; set; }

        public DateTime MaxDate { get; set; }

        public double Avg { get; set; }
    }
}
