using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaStorage.Data.Streaming.Entities
{
    [Table("StreamingSessions", Schema = "StreamingDb")]
    public class StreamingSession
    {
        public StreamingSession()
        {
            DateStarted = DateTime.UtcNow;
            PlayingAtMSec = 0;
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column]
        //[Index("IDX_UserAndStartTime")]
        public Guid UserId { get; set; }

        [Column]
        public bool IsActive { get; set; }

        [Column]
        public Guid? PlayingMediaId { get; set; } // Last playing media identifier.

        [Column]
        public int PlayingAtMSec { get; set; } // Milliseconds paused at.

        [Column]
        public string StateInfoJson { get; set; }

        [Column]
        public DateTime DateStarted { get; set; }

        [Column]
        public DateTime? DateFinished { get; set; }

        [Column, StringLength(32)]
        public string UserIp { get; set; }

        [ForeignKey("UserId")]
        public virtual StreamingUser User { get; set; }
    }
}
