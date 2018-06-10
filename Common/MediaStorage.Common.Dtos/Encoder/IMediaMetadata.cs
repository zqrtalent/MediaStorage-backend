using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.Encoder
{
    public interface IMediaMetadata
    {
        string Artist { get; }
        string Album { get; }
        string Song { get; }
        string Track { get; }
        string Year { get; }
        string Composer { get; }
        string Genre { get; }
        int DurationSec { get; }
        IDictionary<AttachedPictureType, string> AlbumImagesUrl { get; set; }
        string ArtistImageUrl { get; set; }
    }

    public enum AttachedPictureType
    {
        Other = 0,
        FileIcon32x32 = 1,
        CoverFront = 2,
        CoverBack = 3,
        LeafletPage = 4,
        Media = 5,
        LeadArtist = 7,
        Artist = 8,
        Conductor = 9,
        Band = 10,
        Composer = 11,
        Lyricist = 12,
        RecordingLocation = 13,
        DuringRecording = 14,
        DuringPerformance = 15,
        MovieScreenCapture = 16,
        BrightColouredFish = 17,
        Illustration = 18,
        BandLogotype = 19,
        PublisherLogotype = 20
    }

    public class AttachedPicture
    {
        public AttachedPictureType Type { get; set; }
        public string MimeType { get; set; }
        public byte[] PictureData { get; set; }
    }
}
