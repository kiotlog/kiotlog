(*
    Copyright (C) 2017 Giampaolo Mancini, Trampoline SRL.
    Copyright (C) 2017 Francesco Varano, Trampoline SRL.

    This file is part of Kiotlog.

    Kiotlog is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kiotlog is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace KiotlogDBF.Context

open System.Collections.Generic
open Microsoft.EntityFrameworkCore

open KiotlogDBF.Models
open KiotlogDBF.Json

// https://stackoverflow.com/questions/5423768/c-sharp-to-f-ef-code-first
type KiotlogDBFContext (dbContextOptions: DbContextOptions<KiotlogDBFContext>) =
    inherit DbContext (dbContextOptions)

    // https://stackoverflow.com/questions/26775760/how-to-create-a-virtual-record-field-for-entity-framework-lazy-loading
    [<DefaultValue>] val mutable private devices : DbSet<Devices>
    abstract member Devices : DbSet<Devices> with get, set
    override this.Devices with get() = this.devices and set(value) = this.devices <- value

    // abstract member Devices : DbSet<Devices> with get, set
    // default val Devices = DbContext.Set<Devices>() with get, set

    // [<DefaultValue>] val mutable devices : DbSet<Devices>
    // member public this.Devices with get() = this.devices and set(value) = this.devices <- value

    [<DefaultValue>] val mutable private points : DbSet<Points>
    abstract Points : DbSet<Points> with get, set
    override this.Points with get () = this.points and set(value) = this.points <- value

    [<DefaultValue>] val mutable private sensors : DbSet<Sensors>
    abstract Sensors : DbSet<Sensors> with get, set
    override this.Sensors with get() = this.sensors and set(value) = this.sensors <- value

    [<DefaultValue>] val mutable private sensortypes : DbSet<SensorTypes>
    abstract SensorTypes : DbSet<SensorTypes> with get, set
    override this.SensorTypes with get() = this.sensortypes and set(value) = this.sensortypes <- value

    [<DefaultValue>] val mutable conversions : DbSet<Conversions>
    abstract Conversions : DbSet<Conversions> with get, set
    override this.Conversions with get() = this.conversions and set(value) = this.conversions <- value

    override __.OnConfiguring(optionsBuilder: DbContextOptionsBuilder) =
        if not optionsBuilder.IsConfigured then () else ()

    override __.OnModelCreating(modelBuilder: ModelBuilder) =
        modelBuilder.HasPostgresExtension("pgcrypto") |> ignore

        modelBuilder.Entity<Devices>(
            fun entity ->
                entity.ToTable("devices") |> ignore
                entity.HasKey(fun d -> d.Id :> obj)
                    .HasName("devices_pkey") |> ignore
                entity.HasIndex(fun d -> d.Device :> obj)
                    .HasName("devices_device_key").IsUnique |> ignore
                entity.Property(fun d -> d.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()") |> ignore
                entity.Property(fun d -> d.Device)
                    .HasColumnName("device")
                    .IsRequired()
                    .HasDefaultValueSql("device") |> ignore
                entity.Property(fun d -> d.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb")
                    .HasConversion(jsonConverter<DevicesMeta>)
                    .HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property(fun d -> d.Auth)
                    .HasColumnName("auth")
                    .HasColumnType("jsonb")
                    .HasConversion(jsonConverter<DevicesAuth>)
                    .HasDefaultValueSql("json_build_object('klsn', json_build_object('key', encode(gen_random_bytes(32), 'base64')), 'basic', json_build_object('token', encode(gen_random_bytes(32), 'base64')))") |> ignore
                entity.Property(fun d -> d.Frame)
                    .HasColumnName("frame")
                    .HasColumnType("jsonb")
                    .HasConversion(jsonConverter<DevicesFrame>)
                    .HasDefaultValueSql(""" '{"bigendian": true, "bitfields": false}'::jsonb """) |> ignore
        ) |> ignore

        modelBuilder.Entity<Points>(
            fun entity ->
                entity.ToTable("points") |> ignore
                entity.HasKey(fun p -> p.Id :> obj).HasName("points_pkey") |> ignore
                entity.Property(fun p -> p.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()") |> ignore
                entity.Property(fun p -> p.Data)
                    .HasColumnName("data")
                    .HasColumnType("jsonb")
                    .IsRequired()
                    .HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property(fun p -> p.Flags)
                    .HasColumnName("flags")
                    .HasColumnType("jsonb")
                    .IsRequired()
                    .HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property(fun p -> p.Time)
                    .HasColumnName("time")                
                    .HasDefaultValueSql("now()") |> ignore
                entity.Property("DeviceId").HasColumnName("device_id") |> ignore                
                entity.HasOne(fun p -> p.Device)
                    .WithMany(fun (d : Devices) -> d.Points :> IEnumerable<_>)
                    .HasForeignKey(fun (p : Points) -> p.DeviceId :> obj)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("points_device_fkey")
                |> ignore
            ) |> ignore

        modelBuilder.Entity<Sensors>(
            fun entity ->
                entity.ToTable("sensors") |> ignore
                entity.HasKey(fun s -> s.Id :> obj).HasName("sensors_pkey") |> ignore
                entity.Property(fun s -> s.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property(fun s -> s.Fmt)
                    .HasColumnName("fmt")
                    .HasColumnType("jsonb")                    
                    .HasConversion(jsonConverter<SensorsFmt>)
                    .HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property(fun s -> s.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb")
                    .HasConversion(jsonConverter<SensorsMeta>)                    
                    .HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property("DeviceId")
                    .HasColumnName("device_id")
                    .HasColumnType("uuid") |> ignore                
                entity.HasOne(fun s -> s.Device)
                    .WithMany(fun (d : Devices) -> d.Sensors :> IEnumerable<_>)
                    .HasForeignKey(fun (s : Sensors) -> s.DeviceId :> obj)
                    .HasConstraintName("sensors_device_id_fkey") |> ignore
                entity.Property("SensorTypeId").HasColumnName("sensor_type_id") |> ignore                
                entity.HasOne(fun s -> s.SensorType)
                    .WithMany(fun (t : SensorTypes) -> t.Sensors :> IEnumerable<_>)
                    .HasForeignKey(fun (s: Sensors) -> s.SensorTypeId :> obj)
                    .HasConstraintName("sensors_sensor_type_fkey")  |> ignore
                entity.Property("ConversionId").HasColumnName("conversion_id") |> ignore                
                entity.HasOne(fun s -> s.Conversion)
                    .WithMany(fun (c : Conversions) -> c.Sensors :> IEnumerable<_>)
                    .HasForeignKey(fun (s : Sensors) -> s.ConversionId :> obj)
                    .HasConstraintName("sensors_conversion_fkey") |> ignore
            ) |> ignore

        modelBuilder.Entity<SensorTypes>(
            fun entity ->
                entity.ToTable("sensor_types") |> ignore
                entity.HasKey(fun t -> t.Id :> obj).HasName("sensor_types_pkey") |> ignore
                entity.HasIndex(fun t -> t.Name :> obj).HasName("sensor_types_name_key").IsUnique()  |> ignore
                entity.Property(fun t -> t.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property(fun t -> t.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb")
                    .HasConversion(jsonConverter<SensorTypesMeta>)                
                    .HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property(fun t -> t.Name)
                    .HasColumnName("name")
                    .IsRequired()                    
                    .HasDefaultValueSql("'generic'::text") |> ignore
                entity.Property(fun t -> t.Kind)
                    .HasColumnName("kind")
                    .IsRequired()                    
                    .HasDefaultValueSql("'generic'::text") |> ignore
        ) |> ignore

        modelBuilder.Entity<Conversions>(
            fun entity ->
                entity.ToTable("conversions") |> ignore
                entity.HasKey(fun c -> c.Id :> obj).HasName("convertions_pkey") |> ignore
                entity.Property(fun c -> c.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property(fun c -> c.Fun)
                    .HasColumnName("fun")
                    .IsRequired()
                    .HasDefaultValueSql("'id'::text") |> ignore
        ) |> ignore
