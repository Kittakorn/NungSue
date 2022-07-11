using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NungSue.Interfaces;
using System.Drawing;

namespace NungSue.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobClient;

        public BlobService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task<bool> DeleteBlobAsync(string name, string containerName)
        {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(name);
            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> UploadFileBlobAsync(string name, IFormFile file, string containerName)
        {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(name);
            var httpHeaders = new BlobHttpHeaders { ContentType = file.ContentType };
            await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
            return blobClient.Uri.AbsolutePath;
        }

        public async Task<string> UploadFileBlobAsync(string name, string url, string containerName)
        {
            var client = new HttpClient();
            var image = await client.GetAsync(url);
            var contentType = image.Content.Headers.ContentType.ToString();
            var fileExtension = contentType.Split("/")[1];

            if (fileExtension == "jpeg")
                fileExtension = "jpg";
            name += $".{fileExtension}";

            var containerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(name);

            var httpHeaders = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(image.Content.ReadAsStream(), httpHeaders);
            return blobClient.Uri.AbsolutePath;
        }
    }
}