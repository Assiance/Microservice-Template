using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Clients
{
    public interface IGitHaubClient
    {
        Task<object> Get();
        Task<object> SendAsyncDoesGet();
        Task<object> SendAsyncDoesPost();
    }
}