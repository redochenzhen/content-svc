using ContentSvc.Model.Entities;
using System.Security.Claims;

namespace ContentSvc.WebApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateRandomToken(int length);

        int GetMemberId(ClaimsPrincipal user);

        MinioUser GetMinioUser(ClaimsPrincipal user);
    }
}