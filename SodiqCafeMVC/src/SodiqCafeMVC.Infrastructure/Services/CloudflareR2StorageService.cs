using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using SodiqCafeMVC.Application.Interfaces;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class CloudflareR2StorageService : IFileStorageService
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _serviceUrl;
        private readonly string _bucketName;
        private readonly string _publicUrlPrefix;

        public CloudflareR2StorageService(IConfiguration configuration)
        {
            var r2Config = configuration.GetSection("CloudflareR2");
            _accessKey = r2Config["AccessKey"] ?? throw new ArgumentNullException("CloudflareR2:AccessKey");
            _secretKey = r2Config["SecretKey"] ?? throw new ArgumentNullException("CloudflareR2:SecretKey");
            _serviceUrl = r2Config["ServiceUrl"] ?? throw new ArgumentNullException("CloudflareR2:ServiceUrl");
            _bucketName = r2Config["BucketName"] ?? throw new ArgumentNullException("CloudflareR2:BucketName");
            _publicUrlPrefix = r2Config["PublicUrlPrefix"] ?? throw new ArgumentNullException("CloudflareR2:PublicUrlPrefix");
            
            if (!_publicUrlPrefix.EndsWith("/"))
            {
                _publicUrlPrefix += "/";
            }
        }

        private AmazonS3Client GetS3Client()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                ForcePathStyle = true, // Required for Cloudflare R2
            };
            return new AmazonS3Client(_accessKey, _secretKey, config);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var uniqueFileName = Guid.NewGuid().ToString("N") + "_" + fileName;
            
            using var client = GetS3Client();
            var transferUtility = new TransferUtility(client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = uniqueFileName,
                BucketName = _bucketName,
                ContentType = contentType,
                DisablePayloadSigning = true // Recommended for R2 performance
            };

            await transferUtility.UploadAsync(uploadRequest);

            return _publicUrlPrefix + uniqueFileName;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl) || !fileUrl.StartsWith(_publicUrlPrefix)) return;
            
            // Extract the key from the fileUrl
            var fileName = fileUrl.Replace(_publicUrlPrefix, "");
            
            using var client = GetS3Client();
            await client.DeleteObjectAsync(_bucketName, fileName);
        }
    }
}
