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
