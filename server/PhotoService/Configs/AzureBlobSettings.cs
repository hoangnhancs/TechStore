using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoService.Configs
{
    public class AzureBlobSettings
    {
        public required string ConnectionString { get; set; }
        public required string ContainerName { get; set; }
    }
}
