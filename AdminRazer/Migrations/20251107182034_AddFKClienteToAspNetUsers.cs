using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminRazer.Migrations
{
    /// <inheritdoc />
    public partial class AddFKClienteToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdentityUserId",
                table: "Clientes",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_AspNetUsers_IdentityUserId",
                table: "Clientes",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_AspNetUsers_IdentityUserId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_IdentityUserId",
                table: "Clientes");
        }
    }
}
