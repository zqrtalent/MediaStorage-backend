using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("AlbumAndMediaImages", Schema = "MediaDb")]
    public class AlbumAndMediaImage
    {
        public AlbumAndMediaImage()
        {
            Id = Guid.NewGuid();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid AlbumId { get; set; }

        [Column]
        public bool IsCoverImage { get; set; }

        [Column]
        public Guid MediaImageGroupId { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        [ForeignKey("MediaImageGroupId")]
        public virtual MediaImageGroup MediaImageGroup { get; set; }
    }
}
