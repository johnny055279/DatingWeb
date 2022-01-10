using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Imgur.API;
using Microsoft.AspNetCore.Http;

namespace Dating_WebAPI.Interfaces
{
    public interface IPhotosServices
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);

        Task<DeletionResult> DeletePhotoAsync(string publicid);
    }
}