namespace Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct
    );
    Task DeleteAsync(string storagePath);
    Task<byte[]> GetBytesAsync(string storagePath);
}
