using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("ArtistAndMediaImages", Schema = "MediaDb")]
    public class ArtistAndMediaImage
    {
        public ArtistAndMediaImage()
        {
            Id = Guid.NewGuid();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid ArtistId { get; set; }

        [Column]
        public bool IsCoverImage { get; set; }

        [Column]
        public Guid MediaImageGroupId { get; set; }

        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }

        [ForeignKey("MediaImageGroupId")]
        public virtual MediaImageGroup MediaImageGroup { get; set; }
    }
}
