﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NationalInstruments;

namespace TorpedoService.Migrations
{
    [DbContext(typeof(TorpedoContext))]
    partial class TorpedoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.13")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NationalInstruments.Outcome", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("NumberOfRounds")
                        .HasColumnType("int");

                    b.Property<int?>("Player1Id")
                        .HasColumnType("int");

                    b.Property<int?>("Player2Id")
                        .HasColumnType("int");

                    b.Property<int>("WinnerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Player1Id");

                    b.HasIndex("Player2Id");

                    b.HasIndex("WinnerId");

                    b.ToTable("Outcomes");
                });

            modelBuilder.Entity("NationalInstruments.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("NationalInstruments.Stat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Hits")
                        .HasColumnType("int");

                    b.Property<int>("Misses")
                        .HasColumnType("int");

                    b.Property<int?>("OutcomeId")
                        .HasColumnType("int");

                    b.Property<int?>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("SurvivingShipParts")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OutcomeId");

                    b.HasIndex("PlayerId");

                    b.ToTable("Stats");
                });

            modelBuilder.Entity("NationalInstruments.Outcome", b =>
                {
                    b.HasOne("NationalInstruments.Player", "Player1")
                        .WithMany()
                        .HasForeignKey("Player1Id");

                    b.HasOne("NationalInstruments.Player", "Player2")
                        .WithMany()
                        .HasForeignKey("Player2Id");

                    b.HasOne("NationalInstruments.Player", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player1");

                    b.Navigation("Player2");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("NationalInstruments.Stat", b =>
                {
                    b.HasOne("NationalInstruments.Outcome", null)
                        .WithMany("PlayerStats")
                        .HasForeignKey("OutcomeId");

                    b.HasOne("NationalInstruments.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("NationalInstruments.Outcome", b =>
                {
                    b.Navigation("PlayerStats");
                });
#pragma warning restore 612, 618
        }
    }
}
