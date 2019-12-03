using System;
using System.Threading.Tasks;

namespace EfMicroservice.ExternalData.Clients.Interfaces
{
    public interface IGitHaubClient
    {
        Task<object> Get();
        Task<object> SendAsyncDoesGet();
        Task<object> SendAsyncDoesPost();
    }
}