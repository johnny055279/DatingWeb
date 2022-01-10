using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Services
{
    public class PhotoServices : IPhotosServices
    {
        private readonly Cloudinary _cloudinary;

        // 要針對_cloudinary做詳細的設定，使用IOptions去取得設定檔中的資訊。
        public PhotoServices(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.Cloud_Name,
                config.Value.API_Key,
                config.Value.API_Secret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            return uploadResult;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicid)
        {
            var deleteParams = new DeletionParams(publicid);

            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result;
        }
    }
}