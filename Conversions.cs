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
    [Table("conversions")]
    public partial class Conversions
    {
        public Conversions()
        {
            Sensors = new HashSet<Sensors>();
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("fun")]
        public string Fun { get; set; }

        [InverseProperty("Conversion")]
        public ICollection<Sensors> Sensors { get; set; }
    }
}
