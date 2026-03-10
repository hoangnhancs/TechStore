using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PhotoService.DTOs;

namespace PhotoService.Interface
{
    public interface ICloudinaryServices
    {
        Task<PhotoUploadResultDto?> UploadPhoto(IFormFile file, string folder);
        Task<string> DeletePhoto(string publicId);
    }
}