using CurExApi.Domain;
using CurrExApi.Contracts.V1.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurExApi.Services
{
    public interface ICurrExService
    {
        Task<Statistics> GetStatsAsync(GetStatsRequest statsRequest);
    }
}
