using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediaStorage.Common.Extensions;

namespace MediaStorage.Data.Media.Entities
{
    [Table("Albums", Schema = "MediaDb")]
    public class Album
    {
        public Album()
        {
            LastUpdatedUTC = DateTime.UtcNow.ToUnixTimestamp();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [StringLength(128)]
        [Column]
        public string Name { get; set; }

        [Column]
        public int Year { get; set; }

        [Column]
        public int LastUpdatedUTC { get; set; }

        [Column]
        public Guid ArtistId { get; set; }
    
        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }
    }
}
