using EfMicroservice.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EfMicroservice.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20190427041334_Initial")]
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS \""uuid-ossp\"";

                CREATE TABLE IF NOT EXISTS products (
                    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                    name varchar(256) NOT NULL,
                    price NUMERIC NOT NULL,
                    quantity integer NOT NULL
                ); 
			    
			    CREATE TABLE IF NOT EXISTS orders (
                    id serial PRIMARY KEY,
                    product_id uuid references products(id),
                    quantity integer NOT NULL
                );
			    
			    CREATE INDEX IF NOT EXISTS idx_orders_product_id
                    ON orders(product_id);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE orders;
				DROP TABLE products;
            ");
        }
    }
}
