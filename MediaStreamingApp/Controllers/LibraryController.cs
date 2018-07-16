using System;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MediaStreamingApp.Models;
using MediaStorage.Common.Dtos.Upload;
using MediaStorage.Data.WebApp.Entities;
using MediaStorage.Core.Services;
using MediaStorage.IO;

namespace MediaStreamingApp.Controllers
{
    [Authorize]
    [Route("Library")]
    public class LibraryController : Controller
    {
        protected Guid UserId => Guid.Parse(UserManager.GetUserAsync(HttpContext.User).GetAwaiter().GetResult().Id);
        protected UserManager<ApplicationUser> UserManager {get; set;}
        private readonly IMediaUploadService _uploadService;

        public LibraryController(UserManager<ApplicationUser> userManager, IMediaUploadService uploadService, IStorage storage)
        {
            UserManager = userManager;
            _uploadService = uploadService;

            storage.Exists("hello.txt");
        }
        
        [HttpGet]
        [Route("index")]
        public ActionResult Index()
        {
            ViewBag.UserId = UserId;
            return View();
        }

        /// <summary>
        /// Upload media file.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("media/upload")]
        public ActionResult Upload([FromForm(Name = "media")]IFormFile file)
        {
            bool status = false;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var fileContent = reader.ReadToEnd();
                var parsedContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                status = _uploadService.UploadMedia(reader.BaseStream, parsedContentDisposition.FileName.Value.Trim('\"'), UserId);
            }
            
            return Json(new { Status = status });
        }

        [HttpPost]
        [Route("media/upload/sync")]
        public ActionResult GetUploadSyncInfo()
        {
            var result = _uploadService.GetSyncUpload(UserId);
            return Json(new { Result = result });
        }

        [HttpPost]
        [Route("media/upload/complete")]
        public ActionResult CompleteUpload([FromBody]UploadSync request)
        {
            return Json(new { Status = _uploadService.SyncUploadMedia(UserId, request) });
        }
    }
}