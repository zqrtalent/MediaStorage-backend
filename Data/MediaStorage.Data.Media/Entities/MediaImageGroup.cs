using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("MediaImageGroups", Schema = "MediaDb")]
    public class MediaImageGroup
    {
        public MediaImageGroup()
        {
            Id = Guid.NewGuid();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        public Guid UserId { get; set; }

        [Column]
        public bool IsArchived { get; set; }

        [InverseProperty("ImageGroup")]
        public virtual IEnumerable<MediaImage> Images { get; set; }
    }
}
