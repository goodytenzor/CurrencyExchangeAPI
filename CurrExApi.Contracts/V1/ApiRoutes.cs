using System;
using System.Collections.Generic;
using System.Text;

namespace CurrExApi.Contracts.V1
{
    public class ApiRoutes
    {
        public const string Root = "api";

        public const string Version = "v1";

        public const string Base = Root + "/" + Version;

        public static class ExchangeStats
        {
            public const string GetStats = Base + "/stats";
        }

        public static class Welcome
        {
            public const string Greet = Base + "/welcome";
        }
    }
}
