using System;
using System.Collections.Generic;
using System.Text;
using EfMicroservice.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Data.Models
{
    public class ValueEntity : IVersionInfo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
