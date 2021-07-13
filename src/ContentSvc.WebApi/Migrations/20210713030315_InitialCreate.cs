using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContentSvc.WebApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    creator_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 16, nullable: true),
                    desc = table.Column<string>(maxLength: 256, nullable: true),
                    created_date = table.Column<DateTime>(nullable: false),
                    deleted_flag = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "minio_users",
                columns: table => new
                {
                    AccessKey = table.Column<string>(maxLength: 64, nullable: false),
                    secret_key = table.Column<string>(nullable: true),
                    service_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_minio_users", x => x.AccessKey);
                    table.ForeignKey(
                        name: "fk_minio_users_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_minio_users_service_id",
                table: "minio_users",
                column: "service_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "minio_users");

            migrationBuilder.DropTable(
                name: "services");
        }
    }
}
