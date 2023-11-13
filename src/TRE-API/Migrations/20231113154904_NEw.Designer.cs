﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TRE_API.Repositories.DbContexts;

#nullable disable

namespace TRE_API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20231113154904_NEw")]
    partial class NEw
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BL.Models.KeycloakCredentials", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CredentialType")
                        .HasColumnType("integer");

                    b.Property<string>("PasswordEnc")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("KeycloakCredentials");
                });

            modelBuilder.Entity("BL.Models.TESKstatus", b =>
                {
                    b.Property<string>("id")
                        .HasColumnType("text");

                    b.Property<string>("description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("state")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("TESK_Status");
                });

            modelBuilder.Entity("BL.Models.TeskAudit", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("dated")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("subid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("teskid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("TESK_Audit");
                });

            modelBuilder.Entity("BL.Models.TreAuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Approved")
                        .HasColumnType("boolean");

                    b.Property<string>("ApprovedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("IPaddress")
                        .HasColumnType("text");

                    b.Property<int?>("MembershipDecisionId")
                        .HasColumnType("integer");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MembershipDecisionId");

                    b.HasIndex("ProjectId");

                    b.ToTable("TreAuditLogs");
                });

            modelBuilder.Entity("BL.Models.TreMembershipDecision", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ApprovedBy")
                        .HasColumnType("text");

                    b.Property<bool>("Archived")
                        .HasColumnType("boolean");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastDecisionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ProjectExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("UserId");

                    b.ToTable("MembershipDecisions");
                });

            modelBuilder.Entity("BL.Models.TreProject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ApprovedBy")
                        .HasColumnType("text");

                    b.Property<bool>("Archived")
                        .HasColumnType("boolean");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastDecisionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocalProjectName")
                        .HasColumnType("text");

                    b.Property<string>("OutputBucketTre")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<DateTime>("ProjectExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SubmissionBucketTre")
                        .HasColumnType("text");

                    b.Property<int>("SubmissionProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("SubmissionProjectName")
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("BL.Models.TreUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Archived")
                        .HasColumnType("boolean");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<int>("SubmissionUserId")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TREAgent.Repositories.GeneratedRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("GeneratedRole");
                });

            modelBuilder.Entity("TREAgent.Repositories.TokenToExpire", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("SubId")
                        .HasColumnType("integer");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TokensToExpire");
                });

            modelBuilder.Entity("TRE_TESK.Models.RoleData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataToRoles");
                });

            modelBuilder.Entity("BL.Models.TreAuditLog", b =>
                {
                    b.HasOne("BL.Models.TreMembershipDecision", "MembershipDecision")
                        .WithMany("AuditLogs")
                        .HasForeignKey("MembershipDecisionId");

                    b.HasOne("BL.Models.TreProject", "Project")
                        .WithMany("AuditLogs")
                        .HasForeignKey("ProjectId");

                    b.Navigation("MembershipDecision");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("BL.Models.TreMembershipDecision", b =>
                {
                    b.HasOne("BL.Models.TreProject", "Project")
                        .WithMany("MemberDecisions")
                        .HasForeignKey("ProjectId");

                    b.HasOne("BL.Models.TreUser", "User")
                        .WithMany("MemberDecisions")
                        .HasForeignKey("UserId");

                    b.Navigation("Project");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BL.Models.TreMembershipDecision", b =>
                {
                    b.Navigation("AuditLogs");
                });

            modelBuilder.Entity("BL.Models.TreProject", b =>
                {
                    b.Navigation("AuditLogs");

                    b.Navigation("MemberDecisions");
                });

            modelBuilder.Entity("BL.Models.TreUser", b =>
                {
                    b.Navigation("MemberDecisions");
                });
#pragma warning restore 612, 618
        }
    }
}
