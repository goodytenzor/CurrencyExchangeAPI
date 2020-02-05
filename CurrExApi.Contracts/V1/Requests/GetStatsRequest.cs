using System;
using System.Collections.Generic;
using System.Text;

namespace CurrExApi.Contracts.V1.Requests
{
    public class GetStatsRequest
    {
        public string CurrencyConversion { get; set; }

        public IEnumerable<DateTime> Dates { get; set; }
    }
}
