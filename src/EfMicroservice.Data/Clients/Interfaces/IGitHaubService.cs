using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EfMicroservice.Data.Clients.Interfaces
{
    public interface IGitHaubService
    {
        Task<object> Get();
    }
}
