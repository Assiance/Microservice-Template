using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Data.Models
{
    public class ValueEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
