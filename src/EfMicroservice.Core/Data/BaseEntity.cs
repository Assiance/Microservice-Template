using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Core.Data
{
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
