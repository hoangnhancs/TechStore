using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoService.DTOs
{
    public class PhotoUploadResultDto
    {
        public required string PublicId { get; set; }
        public required string Url { get; set; }
    }
}