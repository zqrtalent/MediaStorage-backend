using System;
using System.IO;
using System.Runtime.Serialization;

namespace MediaStorage.IO.GoogleDrive
{
    [DataContract]
    internal class GoogleDriveFileState
    {
        [DataMember]
        public string FileId { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public long Length { get; set; }

        [DataMember]
        public long Offset { get; set; }
    }
}