using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KiotlogDB
{
    public partial class KiotlogDBContext : DbContext
    {
        public virtual DbSet<Conversions> Conversions { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<Points> Points { get; set; }
        public virtual DbSet<Sensors> Sensors { get; set; }
        public virtual DbSet<SensorTypes> SensorTypes { get; set; }

        public KiotlogDBContext(DbContextOptions<KiotlogDBContext> dbContextOptions)
            :base(dbContextOptions)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            { }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pgcrypto");

            modelBuilder.Entity<Conversions>(entity =>
            {
                entity.ToTable("conversions");

                entity.HasIndex(e => e.Id)
                    .HasName("conversions_id_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Fun)
                    .IsRequired()
                    .HasColumnName("fun")
                    .HasDefaultValueSql("'id'::text");
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.ToTable("devices");

                entity.HasIndex(e => e.Device)
                    .HasName("devices_device_key")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("devices_id_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e._Auth)
                    .IsRequired()
                    .HasColumnName("auth")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.Device)
                    .IsRequired()
                    .HasColumnName("device");

                entity.Property(e => e._Frame)
                    .IsRequired()
                    .HasColumnName("frame")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{\"bigendian\": true, \"bitfields\": false}'::jsonb");

                entity.Property(e => e.Meta)
                    .IsRequired()
                    .HasColumnName("meta")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");
            });

            modelBuilder.Entity<Points>(entity =>
            {
                entity.ToTable("points");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Data)
                    .IsRequired()
                    .HasColumnName("data")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.DeviceDevice)
                    .IsRequired()
                    .HasColumnName("device_device");

                entity.Property(e => e.Flags)
                    .IsRequired()
                    .HasColumnName("flags")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .HasColumnType("timestamptz")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.DeviceDeviceNavigation)
                    .WithMany(p => p.Points)
                    .HasPrincipalKey(p => p.Device)
                    .HasForeignKey(d => d.DeviceDevice)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("points_device_fkey");
            });

            modelBuilder.Entity<Sensors>(entity =>
            {
                entity.ToTable("sensors");

                entity.HasIndex(e => e.Id)
                    .HasName("sensors_id_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.ConversionId).HasColumnName("conversion_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e._Fmt)
                    .IsRequired()
                    .HasColumnName("fmt")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e._Meta)
                    .IsRequired()
                    .HasColumnName("meta")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.SensorTypeId).HasColumnName("sensor_type_id");

                entity.HasOne(d => d.Conversion)
                    .WithMany(p => p.Sensors)
                    .HasForeignKey(d => d.ConversionId)
                    .HasConstraintName("sensors_conversion_fkey");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Sensors)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sensors_device_id_fkey");

                entity.HasOne(d => d.SensorType)
                    .WithMany(p => p.Sensors)
                    .HasForeignKey(d => d.SensorTypeId)
                    .HasConstraintName("sensors_sensor_type_fkey");
            });

            modelBuilder.Entity<SensorTypes>(entity =>
            {
                entity.ToTable("sensor_types");

                entity.HasIndex(e => e.Id)
                    .HasName("sensor_types_id_key")
                    .IsUnique();

                entity.HasIndex(e => e.Name)
                    .HasName("sensor_types_name_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e._Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb")
                    .HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasDefaultValueSql("'generic'::text");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasDefaultValueSql("'generic'::text");
            });
        }
    }
}
