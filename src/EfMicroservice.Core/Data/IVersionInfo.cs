using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Core.Data
{
    public interface IVersionInfo
    {
        byte[] RowVersion { get; set; }
    }
}
