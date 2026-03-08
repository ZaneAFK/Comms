using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comms_Server.Migrations
{
	/// <inheritdoc />
	public partial class CommsDb_2 : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "DomainUsers");

			migrationBuilder.AddColumn<DateTime>(
				name: "CreatedTime",
				table: "AspNetUsers",
				type: "timestamp with time zone",
				nullable: false,
				defaultValue: "NOW()");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "CreatedTime",
				table: "AspNetUsers");

			migrationBuilder.CreateTable(
				name: "DomainUsers",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					SecurityUserId = table.Column<Guid>(type: "uuid", nullable: false),
					CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					Username = table.Column<string>(type: "text", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DomainUsers", x => x.Id);
					table.ForeignKey(
						name: "FK_DomainUsers_AspNetUsers_SecurityUserId",
						column: x => x.SecurityUserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_DomainUsers_SecurityUserId",
				table: "DomainUsers",
				column: "SecurityUserId",
				unique: true);
		}
	}
}
