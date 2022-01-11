using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SecurityFinal.Data.Migrations
{
    public partial class MovieTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "AspNetUsers");
        }
    }
}
