using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PhotoService.DTOs;
using PhotoService.Interface;

namespace PhotoService
{
    public class PhotoService : IPhotoService
    {
        private readonly ICloudinaryServices _cloudinaryServices;
        public PhotoService(ICloudinaryServices cloudinaryServices)
        {
            _cloudinaryServices = cloudinaryServices;
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            var result = await _cloudinaryServices.DeletePhoto(publicId);
            return result == "ok";
        }

        public string GetUrl(string publicId)
        {
            throw new NotImplementedException();
        }

        public async Task<PhotoUploadResultDto> UploadAsync(IFormFile file, string folder)
        {
            var result = await _cloudinaryServices.UploadPhoto(file, folder);
            return result ?? throw new Exception("Photo upload failed");
        }
    }
}