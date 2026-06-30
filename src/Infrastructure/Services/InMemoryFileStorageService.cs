using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class InMemoryFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        var key = $"uploads/{Guid.NewGuid():N}/{fileName}";
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        _files[key] = ms.ToArray();
        return Task.FromResult(key);
    }

    public Task DeleteAsync(string storagePath)
    {
        _files.Remove(storagePath);
        return Task.CompletedTask;
    }

    public Task<byte[]> GetBytesAsync(string storagePath)
    {
        return Task.FromResult(_files.GetValueOrDefault(storagePath) ?? []);
    }
}
