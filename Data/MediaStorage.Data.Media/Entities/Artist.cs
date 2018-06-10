using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediaStorage.Common.Extensions;

namespace MediaStorage.Data.Media.Entities
{
    [Table("Artists", Schema = "MediaDb")]
    public class Artist
    {
        public Artist()
        {
            LastUpdatedUTC = DateTime.UtcNow.ToUnixTimestamp();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        [MaxLength(128)]
        public string Name { get; set; }

        [Column]
        public int LastUpdatedUTC { get; set; }

        [Column]
        public Guid GenreId { get; set; }

        [ForeignKey("GenreId")]
        public Genre Genre { get; set; }
    }
}
