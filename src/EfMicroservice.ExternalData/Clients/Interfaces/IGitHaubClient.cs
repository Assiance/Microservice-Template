using System.Threading.Tasks;

namespace EfMicroservice.ExternalData.Clients.Interfaces
{
    public interface IGitHaubClient
    {
        Task<object> Get();
    }
}
