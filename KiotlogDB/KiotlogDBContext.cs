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
                entity.HasIndex(e => e.Id)
                    .HasName("conversions_id_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Fun).HasDefaultValueSql("'id'::text");
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.HasIndex(e => e.Device)
                    .HasName("devices_device_key")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("devices_id_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e._Auth).HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e._Frame).HasDefaultValueSql("'{\"bigendian\": true, \"bitfields\": false}'::jsonb");

                entity.Property(e => e.Meta).HasDefaultValueSql("'{}'::jsonb");
            });

            modelBuilder.Entity<Points>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Data).HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.Flags).HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.Time).HasDefaultValueSql("now()");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Points)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("points_device_fkey");
            });

            modelBuilder.Entity<Sensors>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("sensors_id_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e._Fmt).HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e._Meta).HasDefaultValueSql("'{}'::jsonb");

                entity.HasOne(d => d.Conversion)
                    .WithMany(p => p.Sensors)
                    .HasForeignKey(d => d.ConversionId)
                    .HasConstraintName("sensors_conversion_fkey");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Sensors)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("sensors_device_id_fkey");

                entity.HasOne(d => d.SensorType)
                    .WithMany(p => p.Sensors)
                    .HasForeignKey(d => d.SensorTypeId)
                    .HasConstraintName("sensors_sensor_type_fkey");
            });

            modelBuilder.Entity<SensorTypes>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("sensor_types_id_key")
                    .IsUnique();

                entity.HasIndex(e => e.Name)
                    .HasName("sensor_types_name_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e._Meta).HasDefaultValueSql("'{}'::jsonb");

                entity.Property(e => e.Name).HasDefaultValueSql("'generic'::text");

                entity.Property(e => e.Type).HasDefaultValueSql("'generic'::text");
            });
        }
    }
}
