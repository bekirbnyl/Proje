using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sinema.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVipSeasonalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VipEndDate",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VipPaymentId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VipStartDate",
                table: "Members",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VipEndDate",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "VipPaymentId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "VipStartDate",
                table: "Members");
        }
    }
}
