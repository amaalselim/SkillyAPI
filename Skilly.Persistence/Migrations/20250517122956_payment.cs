using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_chats_AspNetUsers_FirstUserId",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_chats_AspNetUsers_SecondUserId",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_chats_AspNetUsers_UserId",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_chats_AspNetUsers_UserId1",
                table: "chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_chats_ChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_AspNetUsers_UserId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_AspNetUsers_userId",
                table: "offerSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_providerServices_serviceId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_serviceProviders_ProviderId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_serviceProviders_AspNetUsers_UserId",
                table: "serviceProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_userProfiles_AspNetUsers_UserId",
                table: "userProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reviews",
                table: "reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Messages",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chats",
                table: "chats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "reviews",
                newName: "Review");

            migrationBuilder.RenameTable(
                name: "Messages",
                newName: "Message");

            migrationBuilder.RenameTable(
                name: "chats",
                newName: "Chat");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_serviceId",
                table: "Review",
                newName: "IX_Review_serviceId");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_ProviderId",
                table: "Review",
                newName: "IX_Review_ProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SenderId",
                table: "Message",
                newName: "IX_Message_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ReceiverId",
                table: "Message",
                newName: "IX_Message_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChatId",
                table: "Message",
                newName: "IX_Message_ChatId");

            migrationBuilder.RenameIndex(
                name: "IX_chats_UserId1",
                table: "Chat",
                newName: "IX_Chat_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_chats_UserId",
                table: "Chat",
                newName: "IX_Chat_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_chats_SecondUserId",
                table: "Chat",
                newName: "IX_Chat_SecondUserId");

            migrationBuilder.RenameIndex(
                name: "IX_chats_FirstUserId",
                table: "Chat",
                newName: "IX_Chat_FirstUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "User",
                newName: "IX_User_PhoneNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Review",
                table: "Review",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Message",
                table: "Message",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderServiceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestServiceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymobOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_providerServices_ProviderServiceId",
                        column: x => x.ProviderServiceId,
                        principalTable: "providerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payments_requestServices_RequestServiceId",
                        column: x => x.RequestServiceId,
                        principalTable: "requestServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_ProviderServiceId",
                table: "payments",
                column: "ProviderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_RequestServiceId",
                table: "payments",
                column: "RequestServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_User_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_User_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_User_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_User_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_User_FirstUserId",
                table: "Chat",
                column: "FirstUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_User_SecondUserId",
                table: "Chat",
                column: "SecondUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_User_UserId",
                table: "Chat",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_User_UserId1",
                table: "Chat",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Chat_ChatId",
                table: "Message",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_ReceiverId",
                table: "Message",
                column: "ReceiverId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_SenderId",
                table: "Message",
                column: "SenderId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_User_UserId",
                table: "notifications",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_User_userId",
                table: "offerSalaries",
                column: "userId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_providerServices_serviceId",
                table: "Review",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_serviceProviders_ProviderId",
                table: "Review",
                column: "ProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_serviceProviders_User_UserId",
                table: "serviceProviders",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userProfiles_User_UserId",
                table: "userProfiles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_User_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_User_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_User_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_User_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_User_FirstUserId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_User_SecondUserId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_User_UserId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_User_UserId1",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_Chat_ChatId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_ReceiverId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_SenderId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_User_UserId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_User_userId",
                table: "offerSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_providerServices_serviceId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_serviceProviders_ProviderId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_serviceProviders_User_UserId",
                table: "serviceProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_userProfiles_User_UserId",
                table: "userProfiles");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Review",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Message",
                table: "Message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Review",
                newName: "reviews");

            migrationBuilder.RenameTable(
                name: "Message",
                newName: "Messages");

            migrationBuilder.RenameTable(
                name: "Chat",
                newName: "chats");

            migrationBuilder.RenameIndex(
                name: "IX_User_PhoneNumber",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_PhoneNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Review_serviceId",
                table: "reviews",
                newName: "IX_reviews_serviceId");

            migrationBuilder.RenameIndex(
                name: "IX_Review_ProviderId",
                table: "reviews",
                newName: "IX_reviews_ProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_Message_SenderId",
                table: "Messages",
                newName: "IX_Messages_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Message_ReceiverId",
                table: "Messages",
                newName: "IX_Messages_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Message_ChatId",
                table: "Messages",
                newName: "IX_Messages_ChatId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_UserId1",
                table: "chats",
                newName: "IX_chats_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_UserId",
                table: "chats",
                newName: "IX_chats_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SecondUserId",
                table: "chats",
                newName: "IX_chats_SecondUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_FirstUserId",
                table: "chats",
                newName: "IX_chats_FirstUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reviews",
                table: "reviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Messages",
                table: "Messages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chats",
                table: "chats",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chats_AspNetUsers_FirstUserId",
                table: "chats",
                column: "FirstUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_chats_AspNetUsers_SecondUserId",
                table: "chats",
                column: "SecondUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_chats_AspNetUsers_UserId",
                table: "chats",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_chats_AspNetUsers_UserId1",
                table: "chats",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_AspNetUsers_UserId",
                table: "notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_AspNetUsers_userId",
                table: "offerSalaries",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_providerServices_serviceId",
                table: "reviews",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_serviceProviders_ProviderId",
                table: "reviews",
                column: "ProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_serviceProviders_AspNetUsers_UserId",
                table: "serviceProviders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userProfiles_AspNetUsers_UserId",
                table: "userProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
