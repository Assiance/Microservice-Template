using EfMicroservice.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace EfMicroservice.Common.Shared.UserProvider
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserProviderModel GetCurrentUser()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            var userProfileTokenExist = request.Headers.TryGetValue(KnownHttpHeaders.ProfileToken, out StringValues requestHeaderValue);
            var userProviderModel = new UserProviderModel();

            if (!userProfileTokenExist) return userProviderModel;

            var userProfileToken = requestHeaderValue.FirstOrDefault();

            if (string.IsNullOrEmpty(userProfileToken)) return userProviderModel;

            var profileToken = new JwtSecurityTokenHandler().ReadJwtToken(userProfileToken);
            userProviderModel.Name = profileToken.Claims.FirstOrDefault(c => c.Type == UserTokenProviderClaims.Name)?.Value;
            userProviderModel.Email = profileToken.Claims.FirstOrDefault(c => c.Type == UserTokenProviderClaims.Email)?.Value;
            userProviderModel.Nickname = profileToken.Claims.FirstOrDefault(c => c.Type == UserTokenProviderClaims.NickName)?.Value;
            userProviderModel.Picture = profileToken.Claims.FirstOrDefault(c => c.Type == UserTokenProviderClaims.Picture)?.Value;

            return userProviderModel;
        }
    }
}
