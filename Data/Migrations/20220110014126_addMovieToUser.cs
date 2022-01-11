using Microsoft.EntityFrameworkCore.Migrations;

namespace SecurityFinal.Data.Migrations
{
    public partial class addMovieToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MovieId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovieId",
                table: "AspNetUsers");
        }
    }
}
