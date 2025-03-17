using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MormorsBageri.Migrations
{
    /// <inheritdoc />
    public partial class AddLåstColumnToButiker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Namn",
                table: "Produkter",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Telefonnummer",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Fakturaadress",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ButikschefTelefon",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ButikschefNamn",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ButikNummer",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ButikNamn",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "BrödansvarigTelefon",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "BrödansvarigNamn",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Besöksadress",
                table: "Butiker",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Låst",
                table: "Butiker",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Säljare",
                table: "Beställningar",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Beställare",
                table: "Beställningar",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LösenordHash",
                table: "Användare",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Användare",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Användarnamn",
                table: "Användare",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Användare_Användarnamn",
                table: "Användare",
                column: "Användarnamn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Användare_Användarnamn",
                table: "Användare");

            migrationBuilder.DropColumn(
                name: "Låst",
                table: "Butiker");

            migrationBuilder.UpdateData(
                table: "Produkter",
                keyColumn: "Namn",
                keyValue: null,
                column: "Namn",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Namn",
                table: "Produkter",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "Telefonnummer",
                keyValue: null,
                column: "Telefonnummer",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Telefonnummer",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "Fakturaadress",
                keyValue: null,
                column: "Fakturaadress",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Fakturaadress",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "ButikschefTelefon",
                keyValue: null,
                column: "ButikschefTelefon",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ButikschefTelefon",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "ButikschefNamn",
                keyValue: null,
                column: "ButikschefNamn",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ButikschefNamn",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "ButikNummer",
                keyValue: null,
                column: "ButikNummer",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ButikNummer",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "ButikNamn",
                keyValue: null,
                column: "ButikNamn",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ButikNamn",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "BrödansvarigTelefon",
                keyValue: null,
                column: "BrödansvarigTelefon",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "BrödansvarigTelefon",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "BrödansvarigNamn",
                keyValue: null,
                column: "BrödansvarigNamn",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "BrödansvarigNamn",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Butiker",
                keyColumn: "Besöksadress",
                keyValue: null,
                column: "Besöksadress",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Besöksadress",
                table: "Butiker",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Beställningar",
                keyColumn: "Säljare",
                keyValue: null,
                column: "Säljare",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Säljare",
                table: "Beställningar",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Beställningar",
                keyColumn: "Beställare",
                keyValue: null,
                column: "Beställare",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Beställare",
                table: "Beställningar",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Användare",
                keyColumn: "LösenordHash",
                keyValue: null,
                column: "LösenordHash",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "LösenordHash",
                table: "Användare",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Användare",
                keyColumn: "Email",
                keyValue: null,
                column: "Email",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Användare",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Användare",
                keyColumn: "Användarnamn",
                keyValue: null,
                column: "Användarnamn",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Användarnamn",
                table: "Användare",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
