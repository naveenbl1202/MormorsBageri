﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MormorsBageri.Data;

#nullable disable

namespace MormorsBageri.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("MormorsBageri.Models.Användare", b =>
                {
                    b.Property<int>("AnvändareId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("AnvändareId"));

                    b.Property<string>("Användarnamn")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<bool>("Låst")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("LösenordHash")
                        .HasColumnType("longtext");

                    b.Property<int>("Roll")
                        .HasColumnType("int");

                    b.HasKey("AnvändareId");

                    b.HasIndex("Användarnamn")
                        .IsUnique();

                    b.ToTable("Användare");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställning", b =>
                {
                    b.Property<int>("BeställningId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("BeställningId"));

                    b.Property<string>("Beställare")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Beställningsdatum")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("ButikId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PreliminärtLeveransdatum")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Säljare")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.HasKey("BeställningId");

                    b.HasIndex("ButikId");

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

                    b.HasIndex("ProduktId");

                    b.ToTable("Beställningsdetaljer");
                });

            modelBuilder.Entity("MormorsBageri.Models.Butik", b =>
                {
                    b.Property<int>("ButikId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ButikId"));

                    b.Property<string>("Besöksadress")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("BrödansvarigNamn")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("BrödansvarigTelefon")
                        .HasColumnType("longtext");

                    b.Property<string>("ButikNamn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("ButikNummer")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ButikschefNamn")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("ButikschefTelefon")
                        .HasColumnType("longtext");

                    b.Property<string>("Fakturaadress")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<bool?>("Låst")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Telefonnummer")
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
                        .HasColumnType("longtext");

                    b.HasKey("ProduktId");

                    b.ToTable("Produkter");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställning", b =>
                {
                    b.HasOne("MormorsBageri.Models.Butik", "Butik")
                        .WithMany()
                        .HasForeignKey("ButikId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Butik");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställningsdetalj", b =>
                {
                    b.HasOne("MormorsBageri.Models.Beställning", null)
                        .WithMany("Beställningsdetaljer")
                        .HasForeignKey("BeställningId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MormorsBageri.Models.Produkt", "Produkt")
                        .WithMany()
                        .HasForeignKey("ProduktId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Produkt");
                });

            modelBuilder.Entity("MormorsBageri.Models.Beställning", b =>
                {
                    b.Navigation("Beställningsdetaljer");
                });
#pragma warning restore 612, 618
        }
    }
}
