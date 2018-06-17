using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("MediaFileStateInfos", Schema = "MediaDb")]
    public class MediaFileStateInfo
    {
        public MediaFileStateInfo()
        {
            DateCreated = DateTime.Now;
        }

        [Key, Column(Order = 1)]
        public long Id { get; set; }

        [Column]
        public Guid MediaFileId { get; set; }

        [Column]
        [MaxLength(4096)]
        public string StateInfoJson { get; set; }

        [Column]
        public DateTime DateCreated { get; set; }

        [ForeignKey("MediaFileId")]
        public MediaFile MediaFile { get; set; }
    }
}
