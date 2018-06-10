using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Media.Entities
{
    [Table("Genres", Schema = "MediaDb")]
    public class Genre
    {
        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        [MaxLength(128)]
        public string Name { get; set; }
    }
}
