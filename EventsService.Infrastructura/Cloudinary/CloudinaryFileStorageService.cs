// EventsService.Infraestructura/Cloudinary/CloudinaryFileStorageService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using EventsService.Dominio.Interfaces;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "eventos/imagenes"
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
            throw new Exception($"Error al subir imagen a Cloudinary: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "eventos/folletos"
        };

        // IMPORTANTE: sin ct aquí, usa la sobrecarga de RawUploadParams
        RawUploadResult result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Error al subir archivo a Cloudinary: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}