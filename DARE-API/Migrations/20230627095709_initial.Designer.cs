﻿// <auto-generated />
using System;
using BL.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DARE_API.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230627095709_initial")]
    partial class initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BL.Models.Endpoints", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Endpoints");
                });

            modelBuilder.Entity("BL.Models.FormData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FormIoString")
                        .HasColumnType("text");

                    b.Property<string>("FormIoUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("FormData");
                });

            modelBuilder.Entity("BL.Models.Projects", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OutputBucket")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SubmissionBucket")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Projects");
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

                    b.Property<int>("EndPointId")
                        .HasColumnType("integer");

                    b.Property<int?>("ParentID")
                        .HasColumnType("integer");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("SourceCrate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StatusDescription")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SubmittedById")
                        .HasColumnType("integer");

                    b.Property<string>("TesId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TesJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TesName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EndPointId");

                    b.HasIndex("ParentID");

                    b.HasIndex("ProjectId");

                    b.HasIndex("SubmittedById");

                    b.ToTable("Submissions");
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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("ProjectsId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProjectsId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EndpointsProjects", b =>
                {
                    b.Property<int>("EndpointsId")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectsId")
                        .HasColumnType("integer");

                    b.HasKey("EndpointsId", "ProjectsId");

                    b.HasIndex("ProjectsId");

                    b.ToTable("EndpointsProjects");
                });

            modelBuilder.Entity("BL.Models.FormData", b =>
                {
                    b.HasOne("BL.Models.Projects", "Project")
                        .WithOne("FormData")
                        .HasForeignKey("BL.Models.FormData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.User", "User")
                        .WithOne("FormData")
                        .HasForeignKey("BL.Models.FormData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BL.Models.Submission", b =>
                {
                    b.HasOne("BL.Models.Endpoints", "EndPoint")
                        .WithMany("Submissions")
                        .HasForeignKey("EndPointId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.Submission", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentID");

                    b.HasOne("BL.Models.Projects", "Project")
                        .WithMany("Submissions")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.User", "SubmittedBy")
                        .WithMany("Submissions")
                        .HasForeignKey("SubmittedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EndPoint");

                    b.Navigation("Parent");

                    b.Navigation("Project");

                    b.Navigation("SubmittedBy");
                });

            modelBuilder.Entity("BL.Models.User", b =>
                {
                    b.HasOne("BL.Models.Projects", null)
                        .WithMany("Users")
                        .HasForeignKey("ProjectsId");
                });

            modelBuilder.Entity("EndpointsProjects", b =>
                {
                    b.HasOne("BL.Models.Endpoints", null)
                        .WithMany()
                        .HasForeignKey("EndpointsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BL.Models.Projects", null)
                        .WithMany()
                        .HasForeignKey("ProjectsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BL.Models.Endpoints", b =>
                {
                    b.Navigation("Submissions");
                });

            modelBuilder.Entity("BL.Models.Projects", b =>
                {
                    b.Navigation("FormData")
                        .IsRequired();

                    b.Navigation("Submissions");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("BL.Models.Submission", b =>
                {
                    b.Navigation("Children");
                });

            modelBuilder.Entity("BL.Models.User", b =>
                {
                    b.Navigation("FormData")
                        .IsRequired();

                    b.Navigation("Submissions");
                });
#pragma warning restore 612, 618
        }
    }
}
