using System.Threading.Tasks;
using EfMicroservice.Common.Http.Client.Interfaces;

namespace EfMicroservice.ExternalData.Clients.Interfaces
{
    public interface IGitHaubClient
    {
        Task<object> Get();
    }
}
