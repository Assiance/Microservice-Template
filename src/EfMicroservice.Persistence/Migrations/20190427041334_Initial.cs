using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Persistence.Extensions;
using EfMicroservice.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace EfMicroservice.Persistence.Migrations
{

    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190427041334_Initial")]
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.ComposeSqlUp(typeof(Initial));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.ComposeSqlDown((typeof(Initial)));
        }
    }
}


