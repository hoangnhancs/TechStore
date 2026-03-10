using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PhotoService.DTOs;

namespace PhotoService
{
    public interface IPhotoService
    {
        Task<PhotoUploadResultDto> UploadAsync(IFormFile file, string folder);
        Task<bool> DeleteAsync(string publicId);
        string GetUrl(string publicId);
    }
}