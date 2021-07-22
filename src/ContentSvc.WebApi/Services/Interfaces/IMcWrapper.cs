using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Services.Interfaces
{
    public interface IMcWrapper
    {
        void SetAlias();

        void MakeBucket(string bucketName);

        void MakePublicBucket(string bucketName);

        void AddPolicy(string policyName, string policyFilePath);

        string AddBucketPolicy(string bucketName);

        void SetPolicy(string policyName, string accessKey);

        void AddUser(string accessKey, string secretKey);

        void SetDownload(string bucketName);

        void SetPublicDownload(string bucketName);

        void RemovePolicy(string policyName);

        void RemoveBucketPolicy(string bucketName);

        void RemoveUser(string accessKey);

        void ForceRemoveBucket(string bucketName);
    }
}
