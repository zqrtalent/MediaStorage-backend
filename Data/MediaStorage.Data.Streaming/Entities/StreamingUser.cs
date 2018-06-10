using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace MediaStorage.Data.Streaming.Entities
{
    [Table("StreamingUsers", Schema = "StreamingDb")]
    public class StreamingUser
    {
        public StreamingUser()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.UtcNow;
            DateLastActivity = DateCreated;
            PasswordSalt = GenerateSalt(DateTime.UtcNow.ToString());
            IsActive = true;
        }

        private string GenerateSalt(string text)
        {
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes(text));

            var sb = new StringBuilder(hash.Length*2 + 1);
            var btSingleArray = new byte[1];
            foreach(var b in hash)
            {
                btSingleArray[0] = b; 
                sb.Append(BitConverter.ToString(btSingleArray));
            }
            return sb.ToString();
        }

        [Key, Column(Order = 1)]
        public Guid Id { get; set; }

        [Column, MaxLength(64)]
        public string UserName { get; set; }

        [Column, MaxLength(128)]
        public string PasswordHash { get; set; }

        [Column, MaxLength(128)]
        public string PasswordSalt { get; set; }

        [Column]
        public DateTime DateCreated { get; set; }

        [Column]
        public DateTime DateLastActivity { get; set; }

        [Column]
        public bool IsActive { get; set; }
    }
}
