namespace NungSue.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadFileBlobAsync(string name, IFormFile file, string containerName);
        Task<bool> DeleteBlobAsync(string name, string containerName);
    }
}
