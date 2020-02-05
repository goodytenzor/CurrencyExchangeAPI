using CurrExApi.Contracts.V1.Requests;
using CurrExApi.Contracts.V1.Requests.Queries;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurExApi.Utility
{
    public static class Utility
    {
        /// <summary>
        /// Convert the statistics query into a statistics request object.
        /// Validate the parts of the query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>A valid statistics request object</returns>
        public static GetStatsRequest GetStatsRequestFromQuery(GetStatsQuery query)
        {
            GetStatsRequest result = null;

            var currencyLength = query.CurrencyTypes.Trim().Length;

            if(query != null && (currencyLength == 8) && (query.CurrencyTypes.Contains("->")))
            {
                if(query.Dates != null && query.Dates.Length >= 8)
                {
                    var allValidDates = ValidateDates(query.Dates.Split(',').ToList());

                    result = new GetStatsRequest();
                    result.CurrencyConversion = query.CurrencyTypes.ToUpperInvariant();
                    result.Dates = allValidDates;
                }
            }

            return result;
        }



        /// <summary>
        /// Loop through the string list of dates, remove duplicate dates and convert each date string to a date.
        /// Return the list of dates.
        /// </summary>
        /// <param name="datesList"></param>
        /// <returns>List of dates</returns>
        private static IEnumerable<DateTime> ValidateDates(List<string> datesList)
        {
            var result = new List<DateTime>();

            var usefulDatesList = new HashSet<string>(datesList).ToList(); // remove all duplicates

            var parsedDates = new ConcurrentBag<DateTime>();
            Parallel.ForEach(usefulDatesList, (d, state) =>
            {
                DateTime? resultDate = null;
                DateTime parsedDate;

                string strDate = d.Replace("-","");
                string[] format = { "yyyyMMdd" };
                DateTime date;

                if (DateTime.TryParseExact(strDate,
                                           format,
                                           System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.None,
                                           out parsedDate))
                {
                    //valid date
                    resultDate = parsedDate;
                    parsedDates.Add(parsedDate.Date);
                }
                    
                if (resultDate == null)
                    state.Break(); // quit parsing if any date is invalid.
            });

            result = parsedDates.ToList();

            return result;
        }

        /// <summary>
        /// Build the api url for the external data source with the parameters
        /// </summary>
        /// <param name="exchangeRatesApiUrl"></param>
        /// <param name="queryDate"></param>
        /// <param name="currencies"></param>
        /// <returns>parameterized url with query strings</returns>
        public static string GetExchangeRatesApiUrlWithParameters(string exchangeRatesApiUrl, DateTime queryDate, string currencies)
        {
            string date = queryDate.Date.ToShortDateString();
            string nextDate = queryDate.AddDays(1).Date.ToShortDateString();

            return $"{exchangeRatesApiUrl}history?start_at={date}&end_at={nextDate}&symbols=" + currencies.Replace("->", ",");
        }

        /// <summary>
        /// Generic Json deserializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns>A typed object of the json</returns>
        public static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        /// <summary>
        /// Helper method to convert stream to string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Stringified stream contents</returns>
        public static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }

        /// <summary>
        /// Traverse the Json object to recover a key value pair
        /// </summary>
        /// <param name="strData"></param>
        /// <returns>Dictionary of the json key and value</returns>
        public static Dictionary<string, string> GetJsonDataFromString(string strData)
        {
            var result = new Dictionary<string, string>();
            dynamic dataObj = JsonConvert.DeserializeObject(strData);
            var jObj = (JObject)dataObj;

            foreach (JToken token in jObj.Children())
            {
                if (token is JProperty)
                {
                    var prop = token as JProperty;
                    result.Add(prop.Name, prop.Value.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Helper method to generate a cache key based on the current context
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Cache key</returns>
        public static string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();

            keyBuilder.Append($"{request.Path}");

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }

            return keyBuilder.ToString();
        }
    }
}
