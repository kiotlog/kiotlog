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

namespace KiotlogDBF

open Microsoft.EntityFrameworkCore

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
                entity.HasKey("Id").HasName("devices_pkey") |> ignore
                entity.HasIndex("Device").HasName("devices_device_key").IsUnique |> ignore
                entity.Property("Id").HasDefaultValueSql("gen_random_uuid()") |> ignore
                entity.Property("_meta").HasDefaultValueSql("json_build_object('klsn', json_build_object('key', encode(gen_random_bytes(32), 'base64')), 'basic', json_build_object('token', encode(gen_random_bytes(32), 'base64')))") |> ignore
                entity.Property("_auth").HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property("_frame").HasDefaultValueSql("'{\"bigendian\": true, \"bitfields\": false}'::jsonb") |> ignore
        ) |> ignore

        modelBuilder.Entity<Points>(
            fun entity ->
                entity.HasKey("Id").HasName("points_pkey") |> ignore
                entity.Property("Id").HasDefaultValueSql("gen_random_uuid()") |> ignore
                entity.Property("Data").HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property("Flags").HasDefaultValueSql("'{}'::jsonb") |> ignore
                entity.Property("Time").HasDefaultValueSql("now()") |> ignore
                // entity.HasOne(typedefof<Devices>, "Device").WithMany("Points").HasForeignKey("DeviceId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("points_device_fkey") |> ignore
                entity.HasOne(fun p -> p.Device).WithMany("Points").HasForeignKey("DeviceId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("points_device_fkey") |> ignore
            ) |> ignore

        modelBuilder.Entity<Sensors>(
            fun entity ->
                entity.HasKey("Id").HasName("sensors_pkey") |> ignore
                entity.Property("Id").HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property("_fmt").HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property("_meta").HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.HasOne(fun s -> s.Device).WithMany("Sensors").HasForeignKey("DeviceId").HasConstraintName("sensors_device_id_fkey") |> ignore
                entity.HasOne(fun s -> s.SensorType).WithMany("Sensors").HasForeignKey("SensorTypeId").HasConstraintName("sensors_sensor_type_fkey")  |> ignore
                entity.HasOne(fun s -> s.Conversion).WithMany("Sensors").HasForeignKey("ConversionId").HasConstraintName("sensors_conversion_fkey") |> ignore
            ) |> ignore

        modelBuilder.Entity<SensorTypes>(
            fun entity ->
                entity.HasKey("Id").HasName("sensor_types_pkey") |> ignore
                entity.HasIndex("Name").HasName("sensor_types_name_key").IsUnique()  |> ignore
                entity.Property("Id").HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property("_meta").HasDefaultValueSql("'{}'::jsonb")  |> ignore
                entity.Property("Name").HasDefaultValueSql("'generic'::text") |> ignore
                entity.Property("Type").HasDefaultValueSql("'generic'::text") |> ignore
        ) |> ignore

        modelBuilder.Entity<Conversions>(
            fun entity ->
                entity.HasKey("Id").HasName("convertions_pkey") |> ignore
                entity.Property("Id").HasDefaultValueSql("gen_random_uuid()")  |> ignore
                entity.Property("Fun").HasDefaultValueSql("'id'::text") |> ignore
        ) |> ignore
