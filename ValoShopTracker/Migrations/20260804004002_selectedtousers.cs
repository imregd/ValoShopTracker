using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValoShopTracker.Migrations
{
    /// <inheritdoc />
    public partial class selectedtousers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Selected",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Selected",
                table: "Users");
        }
    }
}
