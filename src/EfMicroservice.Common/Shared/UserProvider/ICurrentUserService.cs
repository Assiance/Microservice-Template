namespace EfMicroservice.Common.Shared.UserProvider
{
    public interface ICurrentUserService
    {
        UserProviderModel GetCurrentUser();
    }
}
