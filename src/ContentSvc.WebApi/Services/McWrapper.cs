using ContentSvc.Model.Minio;
using ContentSvc.WebApi.Options;
using ContentSvc.WebApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ContentSvc.WebApi.Services
{
    public class McWrapper : IMcWrapper
    {
        private readonly static string MC = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @".\mc.exe" : "./mc";
        private readonly ILogger _logger;
        private readonly MinioOptions _options;

        public McWrapper(
            ILogger<McWrapper> logger,
            IOptions<MinioOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public void SetAlias()
        {
            var uri = _options.Secure ? $"https://{_options.Endpoint}" : $"http://{_options.Endpoint}";
            RunProcess(MC, $"alias set {_options.Alias} {new Uri(uri)} {_options.AdminAccessKey} {_options.AdminSecretKey}");
        }

        public void MakeBucket(string bucketName)
        {
            RunProcess(MC, $"mb {_options.Alias}/{bucketName}");
        }

        public void MakePublicBucket(string bucketName)
        {
            MakeBucket($"{bucketName}/public");
        }

        public void AddPolicy(string policyName, string policyFilePath)
        {
            RunProcess(MC, $"admin policy add {_options.Alias} {policyName} {policyFilePath}");
        }

        public string AddBucketPolicy(string bucketName)
        {
            var policy = new Policy
            {
                Statement = new List<Statement>
                {
                    new Statement
                    {
                        Effect = Policy.Effect.ALLOW,
                        Action = new List<string>
                        {
                            Policy.Action.LIST_BUCKET,
                            Policy.Action.GET_BUCKET_LOCATION
                        },
                        Resource = new List<string>
                        {
                            Policy.Resource.Bucket(bucketName)
                        }
                    },
                    new Statement
                    {
                        Effect = Policy.Effect.ALLOW,
                        Action = new List<string>
                        {
                            Policy.Action.GET_OBJECT,
                            Policy.Action.PUT_OBJECT
                        },
                        Resource = new List<string>
                        {
                            Policy.Resource.Public(bucketName)
                        }
                    }
                }
            };
            string policyName = $"{bucketName}_usr";
            string policyFilePath = $"./{policyName}.json";
            string json = JsonSerializer.Serialize(policy, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(policyFilePath, json);
            AddPolicy(policyName, policyFilePath);
            File.Delete(policyFilePath);
            return policyName;
        }

        public void SetPolicy(string policyName, string accessKey)
        {
            RunProcess(MC, $"admin policy set {_options.Alias} {policyName} user={accessKey}");
        }

        public void AddUser(string accessKey, string secretKey)
        {
            RunProcess(MC, $"admin user add {_options.Alias} {accessKey} {secretKey}");
        }

        public void SetDownload(string bucketName)
        {
            RunProcess(MC, $"policy set download {_options.Alias}/{bucketName}");
        }

        public void SetPublicDownload(string bucketName)
        {
            SetDownload($"{bucketName}/public");
        }

        public void RemovePolicy(string policyName)
        {
            RunProcess(MC, $"admin policy remove {_options.Alias} {policyName}");
        }

        public void RemoveBucketPolicy(string bucketName)
        {
            string policyName = $"{bucketName}_usr";
            RemovePolicy(policyName);
        }

        public void RemoveUser(string accessKey)
        {
            RunProcess(MC, $"admin user remove {_options.Alias} {accessKey}");
        }

        public void ForceRemoveBucket(string bucketName)
        {
            RunProcess(MC, $"rb {_options.Alias}/{bucketName} --force");
        }

        private int RunProcess(string fileName, string args)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(fileName, args)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    _logger.LogInformation(line);
                }

                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    _logger.LogError(line);
                }

                while (!process.HasExited)
                {
                    process.WaitForExit();
                }

                if (process.ExitCode == 0) return 0;
            }
            throw new Exception($"执行{fileName}时异常，参数：{args}");
        }
    }
}