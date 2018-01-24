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
    [Table("sensor_types")]
    public partial class SensorTypes
    {
        public class JsonBMeta
        {
            public int Max { get; set;}
            public int Min { get; set;}
        }
        public SensorTypes()
        {
            Sensors = new HashSet<Sensors>();
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("meta", TypeName = "jsonb")]
        internal string _Meta { get; set; }

        [NotMapped]
        public JsonBMeta Meta
        {
            get { return _Meta == null ? null : JsonConvert.DeserializeObject<JsonBMeta>(_Meta, JsonSettings.snakeSettings); }
            set { _Meta = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }

        [Required]
        [Column("type")]
        public string Type { get; set; }

        [InverseProperty("SensorType")]
        public ICollection<Sensors> Sensors { get; set; }

        public bool ShouldSerializeSensors()
        {
            return Sensors != null && Sensors.Count > 0;
        }
    }
}
