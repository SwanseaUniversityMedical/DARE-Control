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
    [Migration("20231011135442_ProjectTreDecisions")]
    partial class ProjectTreDecisions
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BL.Models.HistoricStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StatusDescription")
                        .HasColumnType("text");

                    b.Property<int>("SubmissionId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionId");

                    b.ToTable("HistoricStatus");
                });

            modelBuilder.Entity("BL.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Display")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FormData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("MarkAsEmbargoed")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OutputBucket")
                        .HasColumnType("text");

                    b.Property<string>("ProjectDescription")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SubmissionBucket")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Project");
                });

            modelBuilder.Entity("BL.Models.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DockerInputLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastStatusUpdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ParentID")
                        .HasColumnType("integer");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("SourceCrate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StatusDescription")
                        .HasColumnType("text");

                    b.Property<int>("SubmittedById")
                        .HasColumnType("integer");

                    b.Property<string>("TesId")
                        .HasColumnType("text");

                    b.Property<string>("TesJson")
                        .HasColumnType("text");

                    b.Property<string>("TesName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TreId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ParentID");

                    b.HasIndex("ProjectId");

                    b.HasIndex("SubmittedById");

                    b.HasIndex("TreId");

                    b.ToTable("Submission");
                });

            modelBuilder.Entity("BL.Models.SubmissionCredentials", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("PasswordEnc")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SubmissionCredentials");
                });

            modelBuilder.Entity("BL.Models.SubmissionFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("SubmisionBucketFullPath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SubmissionId")
                        .HasColumnType("integer");

                    b.Property<string>("TreBucketFullPath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionId");

                    b.ToTable("SubmissionFile");
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

                    b.Property<string>("teskid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("TESK_Audit");
                });

            modelBuilder.Entity("BL.Models.Tre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("About")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AdminUsername")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FormData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastHeartBeatReceived")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Tre");
                });

            modelBuilder.Entity("BL.Models.TreAuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ApprovedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Decision")
                        .HasColumnType("text");

                    b.Property<string>("IPaddress")
                        .HasColumnType("text");

                    b.HasKey("Id");

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

            modelBuilder.Entity("BL.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FormData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("BL.Models.ViewModels.ProjectTreDecision", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Decision")
                        .HasColumnType("boolean");

                    b.Property<int?>("SubmissionProjId")
                        .HasColumnType("integer");

                    b.Property<int?>("TreId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionProjId");

                    b.HasIndex("TreId");

                    b.ToTable("ProjectTreDecisions");
                });

            modelBuilder.Entity("ProjectTre", b =>
                {
                    b.Property<int>("ProjectsId")
                        .HasColumnType("integer");

                    b.Property<int>("TresId")
                        .HasColumnType("integer");

                    b.HasKey("ProjectsId", "TresId");

                    b.HasIndex("TresId");

                    b.ToTable("ProjectTre");
                });

            modelBuilder.Entity("ProjectUser", b =>
                {
                    b.Property<int>("ProjectsId")
                        .HasColumnType("integer");

                    b.Property<int>("UsersId")
                        .HasColumnType("integer");

                    b.HasKey("ProjectsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("ProjectUser");
                });

            modelBuilder.Entity("BL.Models.HistoricStatus", b =>
                {
                    b.HasOne("BL.Models.Submission", "Submission")
                        .WithMany("HistoricStatuses")
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Submission");
                });

            modelBuilder.Entity("BL.Models.Submission", b =>
                {
                    b.HasOne("BL.Models.Submission", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentID");

                    b.HasOne("BL.Models.Project", "Project")
                        .WithMany("Submissions")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.User", "SubmittedBy")
                        .WithMany("Submissions")
                        .HasForeignKey("SubmittedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.Tre", "Tre")
                        .WithMany("Submissions")
                        .HasForeignKey("TreId");

                    b.Navigation("Parent");

                    b.Navigation("Project");

                    b.Navigation("SubmittedBy");

                    b.Navigation("Tre");
                });

            modelBuilder.Entity("BL.Models.SubmissionFile", b =>
                {
                    b.HasOne("BL.Models.Submission", "Submission")
                        .WithMany("SubmissionFiles")
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Submission");
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

            modelBuilder.Entity("BL.Models.ViewModels.ProjectTreDecision", b =>
                {
                    b.HasOne("BL.Models.Project", "SubmissionProj")
                        .WithMany()
                        .HasForeignKey("SubmissionProjId");

                    b.HasOne("BL.Models.Tre", "Tre")
                        .WithMany()
                        .HasForeignKey("TreId");

                    b.Navigation("SubmissionProj");

                    b.Navigation("Tre");
                });

            modelBuilder.Entity("ProjectTre", b =>
                {
                    b.HasOne("BL.Models.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.Tre", null)
                        .WithMany()
                        .HasForeignKey("TresId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ProjectUser", b =>
                {
                    b.HasOne("BL.Models.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BL.Models.Project", b =>
                {
                    b.Navigation("Submissions");
                });

            modelBuilder.Entity("BL.Models.Submission", b =>
                {
                    b.Navigation("Children");

                    b.Navigation("HistoricStatuses");

                    b.Navigation("SubmissionFiles");
                });

            modelBuilder.Entity("BL.Models.Tre", b =>
                {
                    b.Navigation("Submissions");
                });

            modelBuilder.Entity("BL.Models.TreProject", b =>
                {
                    b.Navigation("MemberDecisions");
                });

            modelBuilder.Entity("BL.Models.TreUser", b =>
                {
                    b.Navigation("MemberDecisions");
                });

            modelBuilder.Entity("BL.Models.User", b =>
                {
                    b.Navigation("Submissions");
                });
#pragma warning restore 612, 618
        }
    }
}
