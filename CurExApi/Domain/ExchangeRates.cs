using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurExApi.Domain
{
    public class ExchangeRates
    {
        public DateTime CurrenyOnDate { get; set; }

        public string FromCurrencyName { get; set; }
        public double FromCurrencyValue { get; set; }


        public string ToCurrencyName { get; set; }
        public double ToCurrencyValue { get; set; }
    }
}
