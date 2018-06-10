using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediaStorage.Common.Extensions;

namespace MediaStorage.Data.Media.Entities
{
    [Table("Songs", Schema = "MediaDb")]
    public class Song
    {
        public Song()
        {
            LastUpdatedUTC = DateTime.UtcNow.ToUnixTimestamp();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        [MaxLength(128)]
        public string Name { get; set; }

        [Column]
        public int TrackNumber { get; set; }

        [Column]
        public int DurationSec { get; set; }

        [Column]
        public int LastUpdatedUTC { get; set; }

        [Column]
        public Guid AlbumId { get; set; }

        [Column]
        public Guid MediaId { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        [ForeignKey("MediaId")]
        public virtual MediaFile Media { get; set; }
    }
}
