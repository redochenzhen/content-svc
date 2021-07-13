using ContentSvc.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using DTO = ContentSvc.Model.Dto;

namespace ContentSvc.Model.Mapping
{
    public static class MinioUserMapping
    {
        public static MinioUser ToEntiry(this DTO.MinioUser minioUserDto)
        {
            return new MinioUser
            {
                AccessKey = minioUserDto.AccessKey,
                SecretKey = minioUserDto.SecretKey
            };
        }

        public static DTO.MinioUser ToDto(this MinioUser minioUser)
        {
            return new DTO.MinioUser
            {
                AccessKey = minioUser.AccessKey,
                SecretKey = minioUser.SecretKey
            };
        }
    }
}