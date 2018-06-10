using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaStorage.Common.Extensions;
using MediaStorage.Common.Dtos.MediaLibrary;
using MediaStorage.Data.Media.Context;
using MediaStorage.Data.Media.Entities;

namespace MediaStorage.Core.Services
{
    internal class MediaLibraryService : IMediaLibraryService
    {
        private readonly IMediaDataContext _dataContext;
        public MediaLibraryService(IMediaDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public MediaLibraryInfo GetLibraryInfo(DateTime? updatedSince = null)
        {
            int updatedSinceUnix = updatedSince == null ? 0 : ((DateTime)updatedSince).ToUniversalTime().ToUnixTimestamp();
            var songs = (from s in _dataContext.Get<Song>()
                         where s.LastUpdatedUTC > updatedSinceUnix
                         select new { Id = s.Id, AlbumId = s.AlbumId, ArtistId = s.Album.ArtistId }).ToList();

            var artistIds = songs.Select(x => x.ArtistId).Distinct();
            var artists = (from art in _dataContext.Get<Artist>()
                           join aai in _dataContext.Get<ArtistAndMediaImage>() on art.Id equals aai.ArtistId into aaiGroup
                           from aai in aaiGroup.DefaultIfEmpty()
                           where artistIds.Contains(art.Id)
                            select new MLArtist
                            {
                                Id = art.Id.ToString(),
                                Name = art.Name,
                                Genre = art.Genre.Name,
                                ArtworkImageId = aai != null ? aai.MediaImageGroupId.ToString() : string.Empty
                            }).ToList();

            foreach (var artistId in artistIds)
            {
                var albumIds = songs.Where(x => x.ArtistId == artistId).Select(x => x.AlbumId).Distinct();
                var albums = (from alb in _dataContext.Get<Album>()
                              join aai in _dataContext.Get<AlbumAndMediaImage>() on alb.Id equals aai.AlbumId into aaiGroup
                              from aai in aaiGroup.DefaultIfEmpty()
                              where albumIds.Contains(alb.Id)
                              select new MLAlbum
                              {
                                  Id = alb.Id.ToString(),
                                  Name = alb.Name,
                                  Year = alb.Year,
                                  ArtworkImageId = aai != null ? aai.MediaImageGroupId.ToString() : string.Empty
                              }).ToList();


                foreach (var albumId in albumIds)
                {
                    var songIds = songs.Where(x => x.AlbumId == albumId).Select(x => x.Id);
                    var albumSongs = (from son in _dataContext.Get<Song>()
                                  where songIds.Contains(son.Id)
                                  select new MLSong
                                  {
                                      Id = son.Id.ToString(),
                                      Name = son.Name,
                                      DurationSec = son.DurationSec,
                                      Track = son.TrackNumber
                                  }).ToList();

                    albums.Find(x => x.Id == albumId.ToString()).Songs = albumSongs;
                }

                artists.Find(x => x.Id == artistId.ToString()).Albums = albums;
            }

            return new MediaLibraryInfo
            {
                Artists = artists,
                Playlists = new List<MLPlaylist>(),
            };
        }
    }
}
