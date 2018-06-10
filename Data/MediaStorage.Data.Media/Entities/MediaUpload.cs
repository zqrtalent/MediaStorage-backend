using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("MediaUploads", Schema = "MediaDb")]
    public class MediaUpload
    {
        public MediaUpload()
        {
            Id = Guid.NewGuid();
            IsSynced = false;
            MediaFiles = new Collection<MediaFile>();
            DateUploaded = DateTime.Now;
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid UserId { get; set; }

        [Column]
        public int NumOfMedia { get; set; }

        [Column]
        public bool IsSynced { get; set; }

        [Column]
        public DateTime DateUploaded { get; set; }

        [Column]
        public DateTime? DateSynced { get; set; }

        [InverseProperty("MediaUpload")]
        public virtual ICollection<MediaFile> MediaFiles { get; set; }
    }
}
