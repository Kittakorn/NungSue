using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NungSue.Interfaces;

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
            var data = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
            return blobClient.Uri.AbsolutePath;
        }
    }
}