using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurExApi.Models
{
    public class ErrorModels
    {
        public class ApiException : Exception
        {
            public int StatusCode { get; set; }

            public string Content { get; set; }
        }

    }
}
