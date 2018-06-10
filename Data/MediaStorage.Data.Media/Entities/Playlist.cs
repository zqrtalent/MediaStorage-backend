using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("Playlists", Schema = "MediaDb")]
    public class Playlist
    {
        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid UserId { get; set; }

        [StringLength(128)]
        [Column]
        //[Index("Idx_Playlist_Name")]
        public string Name { get; set; }

        [Column]
        public Guid ArtistId { get; set; }
    
        [Column]
        public Guid SongId { get; set; }

        [Column]
        public DateTime DateCreated { get; set; }

        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; }
    }
}
