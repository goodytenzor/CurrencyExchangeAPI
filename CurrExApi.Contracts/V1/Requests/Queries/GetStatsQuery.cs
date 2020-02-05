using System;
using System.Collections.Generic;
using System.Text;

namespace CurrExApi.Contracts.V1.Requests.Queries
{
    public class GetStatsQuery
    {
        public string CurrencyTypes { get; set; }

        public string Dates { get; set; }
    }
}
