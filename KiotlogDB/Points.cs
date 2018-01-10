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

namespace KiotlogDB
{
    [Table("points")]
    public partial class Points
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("device_id")]
        public Guid DeviceId { get; set; }

        [Column("time", TypeName = "timestamptz")]
        public DateTime Time { get; set; }

        [Required]
        [Column("flags", TypeName = "jsonb")]
        public string Flags { get; set; }

        [Required]
        [Column("data", TypeName = "jsonb")]
        public string Data { get; set; }

        [ForeignKey("DeviceId")]
        [InverseProperty("Points")]
        public Devices Device { get; set; }
    }
}
