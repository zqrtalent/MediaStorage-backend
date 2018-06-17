using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SkiaSharp;
using MediaStorage.IO;
using MediaStorage.Data.Media.Context;
using MediaStorage.Common.Extensions;
using MediaStorage.Data.Media.Entities;
using MediaStorage.Common.Dtos.Upload;
using MediaStorage.Common.Dtos.Encoder;
using MediaStorage.Encoder.Extensions;
using System.Text;
using Newtonsoft.Json;

namespace MediaStorage.Core.Services
{
    internal class MediaUploadService : IMediaUploadService
    {
        private readonly IStorage _mediaStorage;
        private readonly ILocalStorage _localStorage;
        private readonly IMediaDataContext _mediaDataContext;

        public MediaUploadService(IStorage mediaStorage, ILocalStorage localStorage, IMediaDataContext dataContext)
        {
            _mediaStorage = mediaStorage;
            _localStorage = localStorage;
            _mediaDataContext = dataContext;
        }

        protected string DirDelimiter => (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");

        public bool UploadMedia(Stream media, string fileName, Guid userId, string mediaFormat = MediaFormatExtension.AudioMp3)
        {
            // Get latest not synced upload by the user or create new media upload entry.
            Guid mediaUploadId = (from mu in _mediaDataContext.Get<MediaUpload>()
                                  where mu.UserId == userId && mu.IsSynced == false
                                 select mu.Id).FirstOrDefault();

            // Create media upload entry.
            if(mediaUploadId == Guid.Empty)
            {
                mediaUploadId = Guid.NewGuid();
                _mediaDataContext.Add(new MediaUpload()
                {
                    Id = mediaUploadId,
                    UserId = userId
                });

                _mediaDataContext.SaveChanges();
            }

            // Copy media file under media upload entry.
            string mediaPath = $"{mediaUploadId}{DirDelimiter}{fileName}";
            if (!_localStorage.Store(mediaPath, media, MediaFormatExtension.GetMimeType(mediaFormat)))
                return false;

            // Read metadata from media file and serialize into json.
            var metadataJson = string.Empty;
            using (var mediaFile = _localStorage.Open(mediaPath))
            {
                IEnumerable<AttachedPicture> pictures;
                var metadata = mediaFile.ReadMetadata(mediaFormat, out pictures);

                // Create attached picture files.
                if(pictures != null && pictures.Any())
                {
                    metadata.AlbumImagesUrl = new Dictionary<AttachedPictureType, string>();

                    foreach (var pic in pictures)
                    {
                        string filePath = $"{mediaUploadId}{DirDelimiter}artwork{DirDelimiter}{fileName}-{pic.Type}.jpeg";
                        using(var pictureStream = new MemoryStream(pic.PictureData, false))
                        {
                            if(_localStorage.Store(filePath, pictureStream, MediaFormatExtension.GetMimeType(mediaFormat)))
                            {
                                if(pic.Type == AttachedPictureType.Artist || pic.Type == AttachedPictureType.Band)
                                    metadata.ArtistImageUrl = filePath;
                                else
                                    metadata.AlbumImagesUrl.Add(pic.Type, filePath);
                            }
                        }
                    }
                }

                if (metadata != null)
                    metadataJson = Newtonsoft.Json.JsonConvert.SerializeObject(metadata);
            }
 
            // Create entry in media table using mediaupload id.
            _mediaDataContext.Add(new MediaFile()
            {
                Id = Guid.NewGuid(),
                MediaUploadId = mediaUploadId,
                UserId = userId,
                Format = mediaFormat,
                MediaInfoJson = metadataJson,
                TempUrl = mediaPath,
                FileSize = media.Length,
            });

            // var mediaUpload = _mediaDataContext.Get<MediaUpload>().Where(x => x.Id == mediaUploadId).FirstOrDefault();
            // mediaUpload.NumOfMedia++;
            //_mediaDataContext.Update(mediaUpload);
            
            _mediaDataContext.Update<MediaUpload>( x => x.Id == mediaUploadId, x => x.NumOfMedia ++ );
            _mediaDataContext.SaveChanges();
            return true;
        }

        /// <summary>
        /// Sync uploaded media files.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool SyncUploadMedia(Guid userId, UploadSync uploadInfo)
        {
            // Get latest not synced upload by the user
            var mediaUploadIds = (from mu in _mediaDataContext.Get<MediaUpload>()
                                  where mu.UserId == userId && mu.IsSynced == false
                                  select mu.Id).ToList();

            if (mediaUploadIds == null || !mediaUploadIds.Any())
                return false;

            foreach (var mediaUploadId in mediaUploadIds)
            {
                // Walk through the files and
                IEnumerable<MediaFile> mediaFiles = (from m in _mediaDataContext.Get<MediaFile>()
                                                     where m.MediaUploadId == mediaUploadId
                                                     select m).ToList();
                foreach (var mediaFile in mediaFiles)
                {
                    // Copy media file.
                    var metadata = MediaMetadataExtension.MetadataFromJson(mediaFile.Format, mediaFile.MediaInfoJson);
                    if(metadata != null)
                    {
                        int index = 1;
                        string mediaFilePath = $"{userId}{DirDelimiter}{metadata.Artist}{DirDelimiter}{metadata.Album}{DirDelimiter}{metadata.Song}.{mediaFile.Format}";
                        while(_mediaStorage.Exists(mediaFilePath))
                        {
                            mediaFilePath = $"{userId}{DirDelimiter}{metadata.Artist}{DirDelimiter}{metadata.Album}{DirDelimiter}{metadata.Song}-{index}.{mediaFile.Format}";
                            index++;
                        }

                        // Copy media file from temporary directory to the media storage specific directory.
                        //CopyFileStream(_localStorage, mediaFile.TempUrl, _mediaStorage, mediaFilePath, MediaFormatExtension.GetMimeType(mediaFile.Format));
                        var encoderFilePath = $"{mediaFilePath}.enc.json";
                        SyncMediaFileStream(mediaFile, mediaFilePath, encoderFilePath);

                        mediaFile.Url = mediaFilePath;
                        _mediaDataContext.Update(mediaFile);

                        CreateMediaWithRelationships(metadata, mediaFile.Id, userId, $"{userId}{DirDelimiter}{metadata.Artist}{DirDelimiter}{metadata.Album}{DirDelimiter}", true);
                    }
                }

                // Mark media upload as synced.
                // var mediaUpload = _mediaDataContext.Get<MediaUpload>().Where(x => x.Id == mediaUploadId).FirstOrDefault();
                // mediaUpload.IsSynced = true;
                // _mediaDataContext.Update(mediaUpload);
                _mediaDataContext.Update<MediaUpload>(x => x.Id == mediaUploadId, x => x.IsSynced = true);
                _mediaDataContext.SaveChanges();
            }
          
            return true;
        }

        public IEnumerable<UploadSync> GetSyncUpload(Guid userId)
        {
            // Get latest not synced upload by the user
            var mediaUploadIds = (from mu in _mediaDataContext.Get<MediaUpload>()
                                  where mu.UserId == userId && mu.IsSynced == false
                                  select mu.Id).ToList();

            if (mediaUploadIds == null || !mediaUploadIds.Any())
                return null;

            var result = new List<UploadSync>();
            foreach (var mediaUploadId in mediaUploadIds)
            {
                // Walk through the files and
                IEnumerable<MediaFile> mediaFiles = (from m in _mediaDataContext.Get<MediaFile>()
                                                     where m.MediaUploadId == mediaUploadId && m.IsArchived == false
                                                     select m).ToList();
                var upload = new UploadSync();
                upload.Id = mediaUploadId;

                foreach (var mediaFile in mediaFiles)
                {
                    // Retrieve media metadata.
                    var metadata = MediaMetadataExtension.MetadataFromJson(mediaFile.Format, mediaFile.MediaInfoJson);
                    upload.Merge(mediaFile.Id, metadata);
                }
                result.Add(upload);
            }
            return result;
        }

        private void CopyFileStream(IStorage from, string pathCopy, IStorage to, string pathTo, string mimeType)
        {
            using (var file = from.Open(pathCopy))
            {
                to.Store(pathTo, file, mimeType);
            }
        }

        private void SyncMediaFileStream(MediaFile mediaFile, string copyAsFilePath, string encoderFilePath)
        {
            var fileStates = new MediaFileStateInfos
            {
                MediaFileStateJson = string.Empty,
                EncoderFileStateJson = string.Empty
            };

            using (var file = _localStorage.Open(mediaFile.TempUrl))
            {
                // Generate encoder state and store as json file.
                if(!string.IsNullOrEmpty(encoderFilePath))
                {
                    var encoderStateJson = MediaEncoderExtension.GenerateEncoderState(mediaFile.Format, file, true);
                    if(!string.IsNullOrEmpty(encoderStateJson))
                    {
                        using(var mem = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(encoderStateJson)))
                        {
                            using( var encoderFile = _mediaStorage.StoreAndGetFileStream(encoderFilePath, mem, "text/json"))
                            {
                                fileStates.EncoderFileStateJson = encoderFile.SaveStateAsJson();
                            }
                        }
                    }
                }

                using(var mediaFileStream = _mediaStorage.StoreAndGetFileStream(copyAsFilePath, file, 
                    MediaFormatExtension.GetMimeType(mediaFile.Format)))
                {
                    fileStates.MediaFileStateJson = mediaFileStream.SaveStateAsJson();
                }
            }

            // Add MediaFileStateInfo entry.
            _mediaDataContext.Add(new MediaFileStateInfo
            {
                MediaFileId = mediaFile.Id,
                StateInfoJson = JsonConvert.SerializeObject(fileStates)
            }); 
        }
        
        protected IEnumerable<MediaImage> ResizeAndCopyImage(IStorageFile imageStream, string savePath, string imageName, string mimeType)
        {
            byte[] imageData = new byte[imageStream.Length];
            if (imageStream.Read(imageData, 0, imageData.Length) != imageData.Length)
                return null;

            List<MediaImage> listImages = new List<MediaImage>();
            SkiaImageHelper.ResizeImage(imageData, new[] 
            { 
                new Tuple<int, int>(500, 500), new Tuple<int, int>(300, 300), new Tuple<int, int>(150, 150)
            }.AsEnumerable(),
            (s, w, h) => 
            {
                if(_mediaStorage.Store($"{savePath}{imageName}-{w}x{h}.jpeg", s, mimeType))
                {
                    listImages.Add(new MediaImage
                    {
                        Format = mimeType,
                        ImageSize = (w == 500 ? MediaImageSize.Large : (w == 300 ? MediaImageSize.Medium : MediaImageSize.Small)),
                        Url = $"{savePath}{imageName}-{w}x{h}.jpeg",
                        FileSize = s.Length,
                    });
                }
            },  SKEncodedImageFormat.Jpeg, 85);
            return listImages;
        }
 
        protected void CreateMediaWithRelationships(IMediaMetadata meta, Guid mediaId, Guid userId, string imageSavePath,  bool saveChanges)
        {
            Guid artistCoverMediaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            Guid albumCoverMediaId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            // Create/Update Genre.
            string genreName = meta.Genre;
            if (string.IsNullOrEmpty(genreName))
                genreName = "Unknown";

            var genre = _mediaDataContext.Get<Genre>().Where(x => x.Name == genreName).FirstOrDefault();
            if (genre == null)
            {
                genre = new Genre
                {
                    Id = Guid.NewGuid(),
                    Name = genreName
                };
                _mediaDataContext.Add(genre);
            }

            // Create/Update Artist.
            string artistName = meta.Artist;
            if (string.IsNullOrEmpty(artistName))
                artistName = "Unknown Artist";

            var artist = _mediaDataContext.Get<Artist>().Where(x => x.Name == artistName).FirstOrDefault();
            if (artist == null)
            {
                artist = new Artist
                {
                    Id = Guid.NewGuid(),
                    Name = artistName,
                    GenreId = genre.Id,
                    Genre = genre
                };
                _mediaDataContext.Add(artist);
            }

            // Create/Update Album.
            string albumName = meta.Album;
            if (string.IsNullOrEmpty(albumName))
                albumName = "Unknown Album";

            var album = _mediaDataContext.Get<Album>().Where(x => x.Name == albumName).FirstOrDefault();
            if (album == null)
            {
                int year = 0;
                int.TryParse(meta.Year, out year);

                album = new Album
                {
                    Id = Guid.NewGuid(),
                    Name = albumName,
                    ArtistId = artist.Id,
                    Year = year,
                };

                // Album cover image.
                if(meta.AlbumImagesUrl != null && meta.AlbumImagesUrl.Any())
                {
                    string albumImageUrl = meta.AlbumImagesUrl.FirstOrDefault().Value;
                    IEnumerable<MediaImage> images = null;
                    using (var imageStream = _localStorage.Open(albumImageUrl))
                    {
                        images = ResizeAndCopyImage(imageStream, imageSavePath, "album", MediaFormatExtension.GetMimeType(MediaFormatExtension.ImageJpeg));
                    }

                    if(images != null && images.Any())
                    {
                        var imageGroup = new MediaImageGroup
                        {
                            UserId = userId
                        };
                        _mediaDataContext.Add(imageGroup);
                        _mediaDataContext.Add(new AlbumAndMediaImage
                        {
                            AlbumId = album.Id,
                            IsCoverImage = true,
                            MediaImageGroupId = imageGroup.Id
                        });

                        foreach (var m in images)
                        {
                            m.ImageGroupId = imageGroup.Id;
                            _mediaDataContext.Add(m);
                        }
                    }
                }

                _mediaDataContext.Add(album);
            }

            // Create/Update Song.
            string songName = meta.Song;
            if (string.IsNullOrEmpty(songName))
                albumName = "Unknown Song";

            int trackNumber = 0;
            int.TryParse(meta.Track, out trackNumber);

            var song = new Song
            {
                Id = Guid.NewGuid(),
                Name = songName,
                AlbumId = album.Id,
                TrackNumber = trackNumber,
                DurationSec = meta.DurationSec,
                MediaId = mediaId
            };
            _mediaDataContext.Add(song);
            if (saveChanges)
                _mediaDataContext.SaveChanges();
        }
    }
}