using System;

namespace MediaStorage.Core.Services
{
    public interface IStreamingUserService
    {
        bool CreateUser(string userName, string password);

        bool Authenticate(string userName, string password, string hash, out Guid userId);
    }
}
