﻿// <auto-generated />
using MediaStorage.Data.Streaming.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace MediaStorage.Data.Streaming.Migrations
{
    [DbContext(typeof(StreamingDataContext))]
    partial class StreamingDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("MediaStorage.Data.Streaming.Entities.StreamingSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DateFinished");

                    b.Property<DateTime>("DateStarted");

                    b.Property<bool>("IsActive");

                    b.Property<int>("PlayingAtMSec");

                    b.Property<Guid?>("PlayingMediaId");

                    b.Property<string>("StateInfoJson");

                    b.Property<Guid>("UserId");

                    b.Property<string>("UserIp")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasName("IDX_StreamingSession_User");

                    b.ToTable("StreamingSessions","StreamingDb");
                });

            modelBuilder.Entity("MediaStorage.Data.Streaming.Entities.StreamingUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateLastActivity");

                    b.Property<bool>("IsActive");

                    b.Property<string>("PasswordHash")
                        .HasMaxLength(128);

                    b.Property<string>("PasswordSalt")
                        .HasMaxLength(128);

                    b.Property<string>("UserName")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("StreamingUsers","StreamingDb");
                });

            modelBuilder.Entity("MediaStorage.Data.Streaming.Entities.StreamingSession", b =>
                {
                    b.HasOne("MediaStorage.Data.Streaming.Entities.StreamingUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
