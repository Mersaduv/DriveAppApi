﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdNullableInPhoneVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "PhoneVerifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "PhoneVerifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneVerifications_Users_UserId",
                table: "PhoneVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
