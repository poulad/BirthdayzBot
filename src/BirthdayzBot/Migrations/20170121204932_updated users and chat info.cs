using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BirthdayzBot.Migrations
{
    public partial class updatedusersandchatinfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllMembersAdmin",
                table: "Chats",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ChatType",
                table: "Chats",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserChatStatus",
                table: "Birthdays",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllMembersAdmin",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ChatType",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "UserChatStatus",
                table: "Birthdays");
        }
    }
}
