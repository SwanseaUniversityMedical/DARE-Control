﻿// <auto-generated />
using System;
using DARE_API.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DARE_API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20231019173846_storeforfinalfile")]
    partial class storeforfinalfile
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

            modelBuilder.Entity("BL.Models.AuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FormData")
                        .HasColumnType("text");

                    b.Property<string>("IPaddress")
                        .HasColumnType("text");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<int?>("TestaskId")
                        .HasColumnType("integer");

                    b.Property<int?>("TreId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AuditLogs");
                });

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

                    b.ToTable("HistoricStatuses");
                });

            modelBuilder.Entity("BL.Models.MembershipTreDecision", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<int?>("SubmissionProjId")
                        .HasColumnType("integer");

                    b.Property<int?>("TreId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionProjId");

                    b.HasIndex("TreId");

                    b.HasIndex("UserId");

                    b.ToTable("MembershipTreDecisions");
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

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("BL.Models.ProjectTreDecision", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<int?>("SubmissionProjId")
                        .HasColumnType("integer");

                    b.Property<int?>("TreId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionProjId");

                    b.HasIndex("TreId");

                    b.ToTable("ProjectTreDecisions");
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

                    b.Property<string>("FinalOutputFile")
                        .HasColumnType("text");

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

                    b.ToTable("Submissions");
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

                    b.ToTable("SubmissionFiles");
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

                    b.ToTable("Tres");
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

                    b.ToTable("Users");
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

            modelBuilder.Entity("BL.Models.MembershipTreDecision", b =>
                {
                    b.HasOne("BL.Models.Project", "SubmissionProj")
                        .WithMany("MembershipTreDecision")
                        .HasForeignKey("SubmissionProjId");

                    b.HasOne("BL.Models.Tre", "Tre")
                        .WithMany("MembershipTreDecision")
                        .HasForeignKey("TreId");

                    b.HasOne("BL.Models.User", "User")
                        .WithMany("MembershipTreDecision")
                        .HasForeignKey("UserId");

                    b.Navigation("SubmissionProj");

                    b.Navigation("Tre");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BL.Models.ProjectTreDecision", b =>
                {
                    b.HasOne("BL.Models.Project", "SubmissionProj")
                        .WithMany("ProjectTreDecisions")
                        .HasForeignKey("SubmissionProjId");

                    b.HasOne("BL.Models.Tre", "Tre")
                        .WithMany("ProjectTreDecisions")
                        .HasForeignKey("TreId");

                    b.Navigation("SubmissionProj");

                    b.Navigation("Tre");
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
                    b.Navigation("MembershipTreDecision");

                    b.Navigation("ProjectTreDecisions");

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
                    b.Navigation("MembershipTreDecision");

                    b.Navigation("ProjectTreDecisions");

                    b.Navigation("Submissions");
                });

            modelBuilder.Entity("BL.Models.User", b =>
                {
                    b.Navigation("MembershipTreDecision");

                    b.Navigation("Submissions");
                });
#pragma warning restore 612, 618
        }
    }
}
