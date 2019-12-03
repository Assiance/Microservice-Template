using EfMicroservice.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Omni.BuildingBlocks.Persistence.Extensions;

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


