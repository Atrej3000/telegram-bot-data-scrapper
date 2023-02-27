using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bot.Migrations
{
	/// <inheritdoc />
	public partial class init : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<long>(type: "bigint", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Subscriptions",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					date = table.Column<DateTime>(type: "datetime2", nullable: false),
					query = table.Column<string>(type: "nvarchar(max)", nullable: false),
					UserId = table.Column<long>(type: "bigint", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Subscriptions", x => x.Id);
					table.ForeignKey(
						name: "FK_Subscriptions_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Posts",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Uri = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Price = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Date = table.Column<DateTime>(type: "datetime2", nullable: false),
					SubscriptionId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Posts", x => x.Id);
					table.ForeignKey(
						name: "FK_Posts_Subscriptions_SubscriptionId",
						column: x => x.SubscriptionId,
						principalTable: "Subscriptions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Posts_SubscriptionId",
				table: "Posts",
				column: "SubscriptionId");

			migrationBuilder.CreateIndex(
				name: "IX_Subscriptions_UserId",
				table: "Subscriptions",
				column: "UserId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Posts");

			migrationBuilder.DropTable(
				name: "Subscriptions");

			migrationBuilder.DropTable(
				name: "Users");
		}
	}
}
