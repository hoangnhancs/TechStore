using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PhotoService.Configs;
using PhotoService.DTOs;
using PhotoService.Interface;

namespace PhotoService.Services
{
    public class CloudinaryServices : ICloudinaryServices
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryServices(IOptions<CloudinarySettings> options)
        {
            var account = new Account(
                options.Value.CloudName,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }
        public async Task<string> DeletePhoto(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await  _cloudinary.DestroyAsync(deleteParams);
            if (result.Error != null)
            {
                throw new Exception(result.Error.Message);
            }

            return result.Result;
        }

        public async Task<PhotoUploadResultDto?> UploadPhoto(IFormFile file, string folder)
        {
            if (file.Length > 0)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception(uploadResult.Error.Message);
                }

                return new PhotoUploadResultDto
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.SecureUrl.AbsoluteUri
                };
            }
            return null;
        }
    }
}