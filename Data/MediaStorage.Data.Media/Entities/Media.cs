using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("MediaFiles", Schema = "MediaDb")]
    public class MediaFile
    {
        public MediaFile()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid MediaUploadId { get; set; }

        [Column]
        [MaxLength(16)]
        public string Format { get; set; } // MediaFormat 

        [Column]
        [StringLength(255)]
        public string Url { get; set; }

        [Column]
        [StringLength(255)]
        public string TempUrl { get; set; } // Temporary upload url before sync to the library.

        [Column]
        public long FileSize { get; set; }

        [Column]
        public DateTime DateCreated { get; set; }

        [Column]
        public Guid UserId { get; set; }

        [Column, MaxLength()]
        public string MediaInfoJson { get; set; }

        [Column]
        public bool IsArchived { get; set; }

        [ForeignKey("MediaUploadId")]
        public MediaUpload MediaUpload { get; set; }
    }
}
