using System.Net.Http;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Http.Client.Interfaces
{
    public interface IBaseHttpClient
    {
        Task<TResult> PutAsync<T, TResult>(T item, string url);
        Task<TResult> PostAsync<T, TResult>(T item, string url);
        Task<TResult> PatchAsync<T, TResult>(T item, string url);
        Task<TResult> GetAsync<TResult>(string url);
        Task<TResult> SendAsync<TResult>(HttpRequestMessage request);
    }
}