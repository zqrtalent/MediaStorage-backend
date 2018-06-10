using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    public enum MediaImageSize
    {
        Default = 0,
        Small,
        Medium,
        Large
    }

    [Table("MediaImages", Schema = "MediaDb")]
    public class MediaImage
    {
        public MediaImage()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid ImageGroupId { get; set; }

        [Column]
        [MaxLength(16)]
        public string Format { get; set; } // MediaFormat 

        [Column]
        [StringLength(255)]
        public string Url { get; set; }

        [Column]
        public long FileSize { get; set; }

        [Column]
        public MediaImageSize ImageSize { get; set; }

        [Column]
        public DateTime DateCreated { get; set; }

        [Column]
        public bool IsArchived { get; set; }

        [ForeignKey("ImageGroupId")]
        public virtual MediaImageGroup ImageGroup { get; set; }
    }
}
