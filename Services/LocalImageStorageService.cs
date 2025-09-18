using Microsoft.AspNetCore.Hosting;

namespace Inmobiliaria10.Services
{
    public class LocalImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _env;

        public LocalImageStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Devuelve la URL accesible públicamente
            return $"/uploads/{folder}/{fileName}";
        }
    }
}
