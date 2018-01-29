/*
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
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
    [Table("sensors")]
    public partial class Sensors
    {
        public class JsonBMeta
        {
            public string Name { get; set; }
        }

        public class JsonBFmt
        {
            public int Index { get; set; }
            public string FmtChr { get; set; }
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("meta", TypeName = "jsonb")]
        internal string _Meta { get; set; }

        [Required]
        [Column("fmt", TypeName = "jsonb")]
        internal string _Fmt { get; set; }

        [NotMapped]
        public JsonBMeta Meta
        {
            get { return _Meta == null ? null : JsonConvert.DeserializeObject<JsonBMeta>(_Meta, JsonSettings.snakeSettings); }
            set { _Meta = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }

        [NotMapped]
        public JsonBFmt Fmt
        {
            get { return _Fmt == null ? null : JsonConvert.DeserializeObject<JsonBFmt>(_Fmt, JsonSettings.snakeSettings); }
            set { _Fmt = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }

        [Column("conversion_id")]
        public Guid? ConversionId { get; set; }

        // public bool ShouldSerializeConversionId ()
        // {
        //     return false;
        // }

        [Column("sensor_type_id")]
        public Guid? SensorTypeId { get; set; }

        [Column("device_id")]
        public Guid DeviceId { get; set; }

        [ForeignKey("ConversionId")]
        [InverseProperty("Sensors")]
        public Conversions Conversion { get; set; }
        public bool ShouldSerializeConversion ()
        {
            return false;
        }

        [ForeignKey("DeviceId")]
        [InverseProperty("Sensors")]
        public Devices Device { get; set; }

        [ForeignKey("SensorTypeId")]
        [InverseProperty("Sensors")]
        public SensorTypes SensorType { get; set; }
        public bool ShouldSerializeSensorType ()
        {
            return false;
        }
    }
}
