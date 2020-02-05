using System;
using System.Collections.Generic;
using System.Text;

namespace CurrExApi.Contracts.V1.Responses
{
    public class StatsResponse
    {
        public double Min { get; set; }

        public DateTime MinDate { get; set; }

        public double Max { get; set; }

        public DateTime MaxDate { get; set; }

        public double Avg { get; set; }
    }
}
