using ContentSvc.Model.Entities;
using DTO = ContentSvc.Model.Dto;

namespace ContentSvc.Model.Mapping
{
    public static class ApiKeyMapping
    {
        public static ApiKey ToEntity(this DTO.ApiKey apiKeyDto)
        {
            return new ApiKey
            {
                ExpiredAt = apiKeyDto.ExpiredAt,
                Remarks = apiKeyDto.Remarks,
                Role = apiKeyDto.Role
            };
        }

        public static DTO.ApiKey ToDto(this ApiKey apiKey)
        {
            return new DTO.ApiKey
            {
                Id = apiKey.Id,
                //Key = apiKey.Key,
                DesensitizedKey = Desensitize(apiKey.Key,5),
                CreatedAt = apiKey.CreatedAt,
                ExpiredAt = apiKey.ExpiredAt,
                Remarks = apiKey.Remarks,
                Role = apiKey.Role
            };
        }

        private static string Desensitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length < 8)
            {
                int half = value.Length >> 1;
                return $"{value.Substring(0, half)}{new string('*', value.Length - half)}";
            }
            return $"{value.Substring(0, 4)}{new string('*', value.Length - 8)}{value.Substring(value.Length - 4, 4)}";
        }

        private static string Desensitize(string value, int starLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length < 8)
            {
                int half = value.Length >> 1;
                return $"{value.Substring(0, half)}{new string('*', starLength)}";
            }
            return $"{value.Substring(0, 4)}{new string('*', starLength)}{value.Substring(value.Length - 4, 4)}";
        }
    }
}
