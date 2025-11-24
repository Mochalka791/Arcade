using System;
using Arcade.Data;
using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Arcade.Data.Migrations
{
    [DbContext(typeof(ArcadeDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4")
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Arcade.Data.Entities.User", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime(6)");

                b.Property<string>("Email")
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("varchar(255)");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                b.HasKey("Id");

                b.HasIndex("Username")
                    .IsUnique();

                b.ToTable("users", (string)null);
            });
#pragma warning restore 612, 618
        }
    }
}
