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
