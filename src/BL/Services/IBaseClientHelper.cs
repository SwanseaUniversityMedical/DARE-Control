
namespace BL.Services
{
    public interface IBaseClientHelper
    {



        Task<TOutput?> CallAPI<TInput, TOutput>(string endPoint, TInput model,
            Dictionary<string, string>? paramList = null, bool usePut = false) where TInput : class? where TOutput : class?, new();

        Task<HttpResponseMessage> CallAPI(string endPoint, StringContent jsonString, Dictionary<string, string>? paramList = null,
            bool usePut = false);


        Task<TOutput?> CallAPIWithoutModel<TOutput>(string endPoint, Dictionary<string, string>? paramList = null)
            where TOutput : class?, new();
    }
}
