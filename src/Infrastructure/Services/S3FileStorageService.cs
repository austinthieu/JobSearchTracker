using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public static class S3Extensions
{
    public static async Task EnsureBucketExistsAsync(
        this IAmazonS3 s3,
        string bucketName,
        CancellationToken ct
    )
    {
        if (!(await s3.ListBucketsAsync(ct)).Buckets.Any(b => b.BucketName == bucketName))
            await s3.PutBucketAsync(bucketName, ct);
    }
}

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public S3FileStorageService(IAmazonS3 s3, IConfiguration configuration)
    {
        _s3 = s3;
        _bucketName = configuration["S3:BucketName"] ?? "uploads";
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        var key = $"uploads/{Guid.NewGuid():N}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        };

        try
        {
            await _s3.PutObjectAsync(request, ct);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            await _s3.EnsureBucketExistsAsync(_bucketName, ct);
            await _s3.PutObjectAsync(request, ct);
        }

        return key;
    }

    public async Task DeleteAsync(string storagePath)
    {
        await _s3.DeleteObjectAsync(_bucketName, storagePath);
    }

    public async Task<byte[]> GetBytesAsync(string storagePath)
    {
        using var response = await _s3.GetObjectAsync(_bucketName, storagePath);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
