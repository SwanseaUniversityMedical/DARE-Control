using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public interface IICODAClientHelper : IBaseClientHelper
    {
        Task<TOutput?> CallAPIWithAccessCode<TInput, TOutput>(string endPoint, string AccessCode, TInput model,
    Dictionary<string, string>? paramList = null, bool usePut = false) where TInput : class? where TOutput : class?, new();

        Task<TOutput?> CallAPIWithoutModelWithAccessCode<TOutput>(string endPoint, string AccessCode, Dictionary<string, string>? paramList = null)
            where TOutput : class?, new();
    }
}
