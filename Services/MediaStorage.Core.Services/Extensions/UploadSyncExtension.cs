using System;
using System.Collections.Generic;
using System.Linq;
using MediaStorage.Common.Dtos.Encoder;
using MediaStorage.Common.Dtos.Upload;

namespace MediaStorage.Common.Extensions
{
    public static class UploadSyncExtension
    {
        public static void Merge(this UploadSync upload, Guid mediaId, IMediaMetadata metadata)
        {
            // Artist
            var artist = upload.Artists.Where(x => x.Name == metadata.Artist).FirstOrDefault();
            if(artist == null)
            {
                artist = new UploadArtist
                {
                     Name = metadata.Artist,
                     ArtistImageId = null
                };
                upload.Artists.Add(artist);
            }

            // Album
            var album = artist.Albums.Where(x => x.Name == metadata.Album).FirstOrDefault();
            if (album == null)
            {
                album = new UploadAlbum
                {
                    Name = metadata.Album,
                    Year = metadata.Year,
                    AlbumImageId = null
                };
                artist.Albums.Add(album);
            }

            // Song
            album.Songs.Add(new UploadSong {
                MediaId = mediaId,
                Genre = metadata.Genre,
                Name = metadata.Song,
                Track = metadata.Track
            });

            upload.NumSongs++;
        }
    }
}
