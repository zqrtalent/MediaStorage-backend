using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaStorage.Common.Dtos.Upload
{
    [DataContract]
    public class UploadSync
    {
        public UploadSync()
        {
            Artists = new List<UploadArtist>();
        }

        // public void Merge(Guid mediaId, IMediaMetadata metadata)
        // {
        //     // Artist
        //     var artist = Artists.Where(x => x.Name == metadata.Artist).FirstOrDefault();
        //     if(artist == null)
        //     {
        //         artist = new UploadArtist
        //         {
        //              Name = metadata.Artist,
        //              ArtistImageId = null
        //         };
        //         Artists.Add(artist);
        //     }

        //     // Album
        //     var album = artist.Albums.Where(x => x.Name == metadata.Album).FirstOrDefault();
        //     if (album == null)
        //     {
        //         album = new UploadAlbum
        //         {
        //             Name = metadata.Album,
        //             Year = metadata.Year,
        //             AlbumImageId = null
        //         };
        //         artist.Albums.Add(album);
        //     }

        //     // Song
        //     album.Songs.Add(new UploadSong {
        //         MediaId = mediaId,
        //         Genre = metadata.Genre,
        //         Name = metadata.Song,
        //         Track = metadata.Track
        //     });

        //     NumSongs++;
        // }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public int NumSongs { get; set; }

        [DataMember]
        public IList<UploadArtist> Artists { get; set; }
    }
}
