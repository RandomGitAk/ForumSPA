using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApp.BusinessLogic.Helpers;
public static class FileService
{
    public static string СreateFilePathFromFileName(string fileName, string foldername)
    {
        if (fileName != null && fileName.Contains('\\', StringComparison.Ordinal))
        {
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
        }

        return $"/{foldername}/" + Guid.NewGuid().ToString() + fileName;
    }

    public static async Task SaveFile(string filePath, IFormFile file, IWebHostEnvironment appEnvironment)
    {
        if (appEnvironment != null && file != null)
        {
            using var fileStream = new FileStream(appEnvironment.WebRootPath + filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }
    }
}
