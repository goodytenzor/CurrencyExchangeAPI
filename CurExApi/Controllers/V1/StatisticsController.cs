using CurExApi.Domain;
using CurExApi.Services;
using CurrExApi.Contracts.V1;
using CurrExApi.Contracts.V1.Requests.Queries;
using CurrExApi.Contracts.V1.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CurExApi.Controllers
{

    [ApiController]
    public class StatisticsController : ControllerBase
    {

        private readonly ICurrExService _currExService;

        public StatisticsController(ICurrExService currExService)
        {
            _currExService = currExService;
        }

        [Route(ApiRoutes.Welcome.Greet)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome!");
        }

        [Route(ApiRoutes.ExchangeStats.GetStats)]
        [HttpPost]
        public async Task<IActionResult> Stats([FromBody] GetStatsQuery query)
        {
            var request = Utility.Utility.GetStatsRequestFromQuery(query);

            if (request == null || !request.Dates.Any())
            {
                return BadRequest(new ErrorResponse(new ErrorModel
                {
                    Message = "Invalid request parameters"
                }));
            }

            Statistics stats = await _currExService.GetStatsAsync(request);

            string verdict = "No exchange rate information available for the given dates.";

            if(stats == null)
            {
                return BadRequest(new ErrorResponse(new ErrorModel{Message = "Sorry, unable to process your request at the moment." +
                    "Please contact your administrator."}));
            }
            else if(stats.Min == 0 && stats.Max == 0)
            {
                return Ok(verdict);
            }

            verdict = $"A min rate of {stats.Min} on {stats.MinDate.ToShortDateString()} \n" +
                $"A max rate of {stats.Max} on {stats.MaxDate.ToShortDateString()} \n" +
                $"An average rate of {stats.Avg}";

            return Ok(verdict);

        }




    }
}