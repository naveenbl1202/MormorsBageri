﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MormorsBageri.Data;

#nullable disable

namespace MormorsBageri.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250307105804_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("MormorsBageri.Models.Användare", b =>
                {
                    b.Property<int>("AnvändareId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("AnvändareId"));

                    b.Property<string>("Användarnamn")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("Låst")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("LösenordHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Roll")
                        .HasColumnType("int");

                    b.HasKey("AnvändareId");

                    b.ToTable("Användare");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställning", b =>
                {
                    b.Property<int>("BeställningId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("BeställningId"));

                    b.Property<string>("Beställare")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Beställningsdatum")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("ButikId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PreliminärtLeveransdatum")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Säljare")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("BeställningId");

                    b.ToTable("Beställningar");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställningsdetalj", b =>
                {
                    b.Property<int>("BeställningsdetaljId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("BeställningsdetaljId"));

                    b.Property<int>("Antal")
                        .HasColumnType("int");

                    b.Property<int>("BeställningId")
                        .HasColumnType("int");

                    b.Property<int>("ProduktId")
                        .HasColumnType("int");

                    b.Property<decimal>("Rabatt")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("Styckpris")
                        .HasColumnType("decimal(65,30)");

                    b.HasKey("BeställningsdetaljId");

                    b.HasIndex("BeställningId");

                    b.ToTable("Beställningsdetaljer");
                });

            modelBuilder.Entity("MormorsBageri.Models.Butik", b =>
                {
                    b.Property<int>("ButikId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ButikId"));

                    b.Property<string>("Besöksadress")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("BrödansvarigNamn")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("BrödansvarigTelefon")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ButikNamn")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ButikNummer")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ButikschefNamn")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ButikschefTelefon")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Fakturaadress")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Telefonnummer")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ButikId");

                    b.ToTable("Butiker");
                });

            modelBuilder.Entity("MormorsBageri.Models.Produkt", b =>
                {
                    b.Property<int>("ProduktId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ProduktId"));

                    b.Property<decimal>("Baspris")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("Namn")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ProduktId");

                    b.ToTable("Produkter");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställningsdetalj", b =>
                {
                    b.HasOne("MormorsBageri.Models.Beställning", null)
                        .WithMany("Beställningsdetaljer")
                        .HasForeignKey("BeställningId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställning", b =>
                {
                    b.Navigation("Beställningsdetaljer");
                });
#pragma warning restore 612, 618
        }
    }
}
