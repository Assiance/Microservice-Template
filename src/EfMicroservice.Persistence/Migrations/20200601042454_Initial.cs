using System;
using EfMicroservice.Domain.Orders;
using EfMicroservice.Domain.Products;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace EfMicroservice.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_statuses",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 256, nullable: false),
                    description = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_statuses",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 256, nullable: false),
                    description = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(maxLength: 256, nullable: false),
                    price = table.Column<decimal>(nullable: false),
                    quantity = table.Column<int>(nullable: false),
                    status_id = table.Column<int>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: true),
                    created_date = table.Column<DateTimeOffset>(nullable: false),
                    modified_date = table.Column<DateTimeOffset>(nullable: true),
                    created_by = table.Column<string>(nullable: false),
                    modified_by = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_product_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "product_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<Guid>(nullable: false),
                    quantity = table.Column<int>(nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: true),
                    status_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_order_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "order_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_product_id",
                table: "orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_id",
                table: "orders",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_status_id",
                table: "products",
                column: "status_id");

            migrationBuilder.InsertData("product_statuses", new[] { "id", "name", "description" }, new[] { ((int)ProductStatuses.InStock).ToString(), ProductStatuses.InStock.ToString(), string.Empty });
            migrationBuilder.InsertData("product_statuses", new[] { "id", "name", "description" }, new[] { ((int)ProductStatuses.OutOfStock).ToString(), ProductStatuses.OutOfStock.ToString(), string.Empty });
            migrationBuilder.InsertData("product_statuses", new[] { "id", "name", "description" }, new[] { ((int)ProductStatuses.Discontinued).ToString(), ProductStatuses.Discontinued.ToString(), string.Empty });

            migrationBuilder.InsertData("order_statuses", new[] { "id", "name", "description" }, new[] { ((int)OrderStatus.Submitted).ToString(), OrderStatus.Submitted.ToString(), string.Empty });
            migrationBuilder.InsertData("order_statuses", new[] { "id", "name", "description" }, new[] { ((int)OrderStatus.Paid).ToString(), OrderStatus.Paid.ToString(), string.Empty });
            migrationBuilder.InsertData("order_statuses", new[] { "id", "name", "description" }, new[] { ((int)OrderStatus.Shipped).ToString(), OrderStatus.Shipped.ToString(), string.Empty });
            migrationBuilder.InsertData("order_statuses", new[] { "id", "name", "description" }, new[] { ((int)OrderStatus.Cancelled).ToString(), OrderStatus.Cancelled.ToString(), string.Empty });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "order_statuses");

            migrationBuilder.DropTable(
                name: "product_statuses");
        }
    }
}
