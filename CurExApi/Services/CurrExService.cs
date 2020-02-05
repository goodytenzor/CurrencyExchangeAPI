using CurExApi.Domain;
using CurrExApi.Contracts.V1.Requests;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static CurExApi.Models.ErrorModels;

namespace CurExApi.Services
{
    public class CurrExService : ICurrExService
    {
        protected readonly IResponseCacheService _responseCacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string _exchangeRatesApiUrl = "https://api.exchangeratesapi.io/";
        private string _cacheKey;
        public CurrExService(IResponseCacheService responseCacheService, IHttpContextAccessor httpContextAccessor)
        {
            _responseCacheService = responseCacheService;
            _httpContextAccessor = httpContextAccessor;

            var context = _httpContextAccessor.HttpContext;
            _cacheKey = Utility.Utility.GenerateCacheKeyFromRequest(context.Request);
        }

        /// <summary>
        /// Check if the dates are available in the cache. 
        /// Y: serve from cache, N: make a new request to external api, then cache them as well.
        /// Filter the data.
        /// From the cached results calculate the Min,Max and Avg statistics for the dates.
        /// </summary>
        /// <param name="statsRequest"></param>
        /// <returns>Statistics for provided dates</returns>
        ///

        public async Task<Statistics> GetStatsAsync(GetStatsRequest statsRequest)
        {
            List<ExchangeRates> poolOfExchangeRatesFromCache = new List<ExchangeRates>();

            var cachedRespStr = await _responseCacheService.GetCachedResponseAsync(_cacheKey);

            if(!String.IsNullOrEmpty(cachedRespStr))
            {
                poolOfExchangeRatesFromCache = JsonConvert.DeserializeObject<List<ExchangeRates>>(cachedRespStr);

                poolOfExchangeRatesFromCache = poolOfExchangeRatesFromCache.Where(d => statsRequest.Dates.Contains(d.CurrenyOnDate)).ToList();
            }

            // Get only the non-cached dates
            statsRequest.Dates = statsRequest.Dates.Except(poolOfExchangeRatesFromCache.Select(d => d.CurrenyOnDate));


            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;


            // Get new exchange rates from external source
            foreach(var queryDate in statsRequest.Dates)
            {
                string apiUrl = Utility.Utility.GetExchangeRatesApiUrlWithParameters(_exchangeRatesApiUrl, queryDate, statsRequest.CurrencyConversion);
                
                var currencyRatesForDate = await GetExchangeRateDataFromStreamAsync(token, apiUrl);

                // Remove the entries that don't have exchange rates for the queried date
                currencyRatesForDate = currencyRatesForDate.Where(c => c.CurrenyOnDate == queryDate).ToList();

                if(currencyRatesForDate.Any())
                    // Store this value in the cache
                    await _responseCacheService.CacheResponseAsync(_cacheKey, currencyRatesForDate, new TimeSpan(0, 1, 0)); // cache it for 1 minute

                // Merge the newly cached results with the existing results
                poolOfExchangeRatesFromCache.AddRange(currencyRatesForDate);
            }

            if (!poolOfExchangeRatesFromCache.Any())
                return new Statistics();

            var leastValue = poolOfExchangeRatesFromCache
                .OrderBy(o => o.CurrenyOnDate)
                .Min(m => m.ToCurrencyValue);

            var leastValueDate = poolOfExchangeRatesFromCache
                .OrderBy(o => o.CurrenyOnDate)
                .Where(m => m.ToCurrencyValue == leastValue)
                .Select(m => m.CurrenyOnDate)
                .FirstOrDefault();

            var maxValue = poolOfExchangeRatesFromCache
                .OrderBy(o => o.CurrenyOnDate)
                .Max(m => m.ToCurrencyValue);

            var maxValueDate = poolOfExchangeRatesFromCache
                .OrderBy(o => o.CurrenyOnDate)
                .Where(m => m.ToCurrencyValue == maxValue)
                .Select(m => m.CurrenyOnDate)
                .FirstOrDefault();

            var avgValue = poolOfExchangeRatesFromCache
                .Average(m => m.ToCurrencyValue);


            var stats = new Statistics()
            {
                Min = leastValue,
                MinDate = leastValueDate,
                Max = maxValue,
                MaxDate = maxValueDate,
                Avg = avgValue
            };



            return stats;
        }

        /// <summary>
        /// Make Http calls to the external API. Retreive json information for processing
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="Url"></param>
        /// <returns>A serialized list of exchange rates</returns>
        private static async Task<List<ExchangeRates>> GetExchangeRateDataFromStreamAsync(CancellationToken cancellationToken, string Url)
        {
            List<ExchangeRates> exchangeRatesForDates = new List<ExchangeRates>();

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    return await ExtractDataFromStreamAsync(exchangeRatesForDates, stream);
                }

                // Something went wrong, show an error message
                var content = await Utility.Utility.StreamToStringAsync(stream);

                throw new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
        }


        /// <summary>
        /// Read data from the stream and extract useful information from it
        /// </summary>
        /// <param name="exchangeRatesForDates"></param>
        /// <param name="stream"></param>
        /// <returns>A list of exchange rates retrieved from the api</returns>
        private static async Task<List<ExchangeRates>> ExtractDataFromStreamAsync(List<ExchangeRates> exchangeRatesForDates, Stream stream)
        {
            var jsonString = await Utility.Utility.StreamToStringAsync(stream);

            Dictionary<string, string> rates = Utility.Utility.GetJsonDataFromString(jsonString);

            foreach (var rateValues in rates.Values)
            {
                Dictionary<string, string> dates = Utility.Utility.GetJsonDataFromString(rateValues);

                foreach (var dateValues in dates.Values)
                {
                    Dictionary<string, string> currencies = Utility.Utility.GetJsonDataFromString(dateValues);

                    var exRate = new ExchangeRates()
                    {
                        CurrenyOnDate = Convert.ToDateTime(dates.Keys.First()),
                        FromCurrencyName = currencies.Keys.First().ToString(),
                        FromCurrencyValue = Convert.ToDouble(currencies.Values.First()),
                        ToCurrencyName = currencies.Keys.Last().ToString(),
                        ToCurrencyValue = Convert.ToDouble(currencies.Values.Last())
                    };

                    exchangeRatesForDates.Add(exRate);

                }

                break; // we only need the rates object from rates.values
            }

            return exchangeRatesForDates;
        }
    }
}
