using CurrExApi.Contracts.V1.Requests;
using CurrExApi.Contracts.V1.Responses;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurExApi.URIs
{
    interface ICurrExApi
    {
        [Post("/api/v1/stats")]
        Task<ApiResponse<Response<StatsResponse>>> GetStatsAsync([Body] GetStatsRequest createStatsRequest);
    }
}
