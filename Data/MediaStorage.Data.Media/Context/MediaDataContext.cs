using System;
using MediaStorage.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MediaStorage.Data.Media.Entities;

namespace MediaStorage.Data.Media.Context
{
    public class MediaDataContext : EfDataContext, IMediaDataContext //ApplicationDataContext
    {
        private readonly IConfiguration _configuration;
        public MediaDataContext(IConfiguration configuration) : base()
        {
            _configuration = configuration;
        }

        public MediaDataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MediaFile> Media { get; set; }
        public DbSet<Song> Song { get; set; }
        public DbSet<Artist> Artist { get; set; }
        public DbSet<ArtistAndMediaImage> ArtistAndMediaImages { get; set; }
        public DbSet<Playlist> Playlist { get; set; }
        public DbSet<Album> Album { get; set; }
        public DbSet<AlbumAndMediaImage> AlbumAndMediaImages { get; set; }
        public DbSet<MediaImage> MediaImages { get; set; }
        public DbSet<MediaImageGroup> MediaImageGroups { get; set; }
        public DbSet<Genre> Genre { get; set; }
        public DbSet<MediaUpload> MediaUpload { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Album>().HasIndex(e=> e.Name).HasName(@"Idx_Album_Name").IsUnique(false);
            builder.Entity<Album>().HasIndex(e=> e.LastUpdatedUTC).HasName(@"Idx_Album_LastUpdated").IsUnique(false);
            
            builder.Entity<Artist>().HasIndex(e=> e.Name).HasName(@"Idx_Artist_Name").IsUnique(false);
            builder.Entity<Artist>().HasIndex(e=> e.LastUpdatedUTC).HasName(@"Idx_Artist_LastUpdated").IsUnique(false);
            
            builder.Entity<MediaFile>().HasIndex(e=> e.Format).IsUnique(false);
            builder.Entity<MediaImage>().HasIndex(e=> e.ImageGroupId).IsUnique(false);
            
            builder.Entity<Playlist>().HasIndex(e=> e.Name).HasName(@"Idx_Playlist_Name").IsUnique(false);
            builder.Entity<Song>().HasIndex(e=> e.LastUpdatedUTC).HasName(@"Idx_Song_LastUpdated").IsUnique(false);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_configuration.GetConnectionString("MediaDataConnectionString"));
            }
        }
    }

}