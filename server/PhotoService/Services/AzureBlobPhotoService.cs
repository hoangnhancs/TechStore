using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PhotoService.Configs;
using PhotoService.DTOs;

namespace PhotoService.Services
{
    public class AzureBlobPhotoService : IPhotoService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobPhotoService(IOptions<AzureBlobSettings> options)
        {
            _containerClient = new BlobContainerClient(options.Value.ConnectionString, options.Value.ContainerName);
        }

        public async Task<PhotoUploadResultDto> UploadAsync(IFormFile file, string folder)
        {
            if (file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            var blobName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });

            return new PhotoUploadResultDto
            {
                PublicId = blobName,
                Url = blobClient.Uri.AbsoluteUri
            };
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            var response = await _containerClient.GetBlobClient(publicId).DeleteIfExistsAsync();
            return response.Value;
        }

        public string GetUrl(string publicId) => _containerClient.GetBlobClient(publicId).Uri.AbsoluteUri;
    }
}
