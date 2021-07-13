using System;
using System.Collections.Generic;
using System.Text;

namespace ContentSvc.WebApi.Minio
{
    public class Policy
    {
        public string Version { get; set; } = "2012-10-17";

        public List<Statement> Statement { get; set; }


        public static class Effect
        {
            public const string ALLOW = "Allow";
            public const string DENY = "Deny";
        }

        public static class Action
        {
            public const string GET_OBJECT = "s3:GetObject";  
            public const string PUT_OBJECT = "s3:PutObject";
            public const string LIST_BUCKET = "s3:ListBucket";
            public const string GET_BUCKET_LOCATION = "s3:GetBucketLocation";
        }

        public static class Resource
        {
            public static string Bucket(string bucketName)
            {
                return $"arn:aws:s3:::{bucketName}/*";
            }

            public static string Public(string bucketName)
            {
                return $"arn:aws:s3:::{bucketName}/public/*";
            }
        }
    }

    public class Statement
    {
        public string Effect { get; set; }

        public List<string> Action { get; set; }

        public List<string> Resource { get; set; }
    }

}
