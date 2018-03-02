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

open KiotlogDBF.Models
open Microsoft.EntityFrameworkCore
open System.Collections.Generic

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
                entity.HasKey(fun d -> d.Id :> obj).HasName("devices_pkey") |> ignore
                entity.HasIndex(fun d -> d.Device :> obj).HasName("devices_device_key").IsUnique |> ignore
                entity.Property(fun d -> d.Id).HasDefaultValueSql("gen_random_uuid()") |> ignore
                entity.Property(fun d -> d._Meta).HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property(fun d -> d._Auth).HasDefaultValueSql("json_build_object('klsn', json_build_object('key', encode(gen_random_bytes(32), 'base64')), 'basic', json_build_object('token', encode(gen_random_bytes(32), 'base64')))") |> ignore
                entity.Property(fun d -> d._Frame).HasDefaultValueSql("'{\"bigendian\": true, \"bitfields\": false}'::jsonb") |> ignore
        ) |> ignore

        modelBuilder.Entity<Points>(
            fun entity ->
                entity.HasKey(fun p -> p.Id :> obj).HasName("points_pkey") |> ignore
                entity.Property(fun p -> p.Id).HasDefaultValueSql("gen_random_uuid()") |> ignore
                entity.Property(fun p -> p.Data).HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property(fun p -> p.Flags).HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property(fun p -> p.Time).HasDefaultValueSql("now()") |> ignore
                entity.HasOne(fun p -> p.Device)
                    .WithMany(fun (d : Devices) -> d.Points :> IEnumerable<Points>)
                    .HasForeignKey(fun (p : Points) -> p.DeviceId :> obj)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("points_device_fkey")
                |> ignore
            ) |> ignore

        modelBuilder.Entity<Sensors>(
            fun entity ->
                entity.HasKey(fun s -> s.Id :> obj).HasName("sensors_pkey") |> ignore
                entity.Property(fun s -> s.Id).HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property(fun s -> s._Fmt).HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property(fun s -> s._Meta).HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.HasOne(fun s -> s.Device)
                    .WithMany(fun (d : Devices) -> d.Sensors :> IEnumerable<Sensors>)
                    .HasForeignKey(fun (s : Sensors) -> s.DeviceId :> obj)
                    .HasConstraintName("sensors_device_id_fkey") |> ignore
                entity.HasOne(fun s -> s.SensorType)
                    .WithMany(fun (t : SensorTypes) -> t.Sensors :> IEnumerable<Sensors>)
                    .HasForeignKey(fun (s: Sensors) -> s.SensorTypeId :> obj)
                    .HasConstraintName("sensors_sensor_type_fkey")  |> ignore
                entity.HasOne(fun s -> s.Conversion)
                    .WithMany(fun (c : Conversions) -> c.Sensors :> IEnumerable<Sensors>)
                    .HasForeignKey(fun (s : Sensors) -> s.ConversionId :> obj)
                    .HasConstraintName("sensors_conversion_fkey") |> ignore
            ) |> ignore

        modelBuilder.Entity<SensorTypes>(
            fun entity ->
                entity.HasKey(fun t -> t.Id :> obj).HasName("sensor_types_pkey") |> ignore
                entity.HasIndex(fun t -> t.Name :> obj).HasName("sensor_types_name_key").IsUnique()  |> ignore
                entity.Property(fun t -> t.Id).HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property(fun t -> t._Meta).HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property(fun t -> t.Name).HasDefaultValueSql("'generic'::text") |> ignore
                entity.Property(fun t -> t.Kind).HasDefaultValueSql("'generic'::text") |> ignore
        ) |> ignore

        modelBuilder.Entity<Conversions>(
            fun entity ->
                entity.HasKey(fun c -> c.Id :> obj).HasName("convertions_pkey") |> ignore
                entity.Property(fun c -> c.Id).HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property(fun c -> c.Fun).HasDefaultValueSql("'id'::text") |> ignore
        ) |> ignore
