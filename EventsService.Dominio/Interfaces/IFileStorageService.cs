using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Dominio.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    }
}
