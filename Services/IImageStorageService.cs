namespace Inmobiliaria10.Services
{
    public interface IImageStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}
