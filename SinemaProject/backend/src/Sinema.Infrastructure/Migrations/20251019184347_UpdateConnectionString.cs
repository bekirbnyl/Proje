using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinema.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConnectionString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Members_VipStatus_VipEndDate",
                table: "Members",
                columns: new[] { "VipStatus", "VipEndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_VipStatus_VipEndDate",
                table: "Members");
        }
    }
}
