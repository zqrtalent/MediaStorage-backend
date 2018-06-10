using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MediaStorage.IO;
using MediaStorage.Data.Streaming.Entities;
using MediaStorage.Common.Extensions;
using MediaStorage.Data.Streaming.Context;

namespace MediaStorage.Core.Services
{
    internal class StreamingUserService : IStreamingUserService
    {
        private readonly IStreamingDataContext _dataContext;
        private readonly IStorage _storage;

        public StreamingUserService(IStorage mediaStorage, IStreamingDataContext dataContext)
        {
            _dataContext = dataContext;
            _storage = mediaStorage;
        }

        /// <summary>
        /// Creates streaming user.
        /// </summary>
        /// <param name="userName">User name string.</param>
        /// <param name="password">Password string.</param>
        /// <returns></returns>
        public bool CreateUser(string userName, string password)
        {
            var userNameValidation = new Regex("[a-z,A-Z,0-9,_!@#$%^&*]{4,32}", RegexOptions.Compiled|RegexOptions.Singleline);
            if (string.IsNullOrEmpty(userName) || 
                !userNameValidation.IsMatch(userName, 0) ||
                string.IsNullOrEmpty(password) ||
                password.Length < 6)
                throw new InvalidOperationException(@"Invalid username, allowed characters are 'a-z,A-Z,0-9,_!@#$%^&*' and length between 4 and 32 !");

            if (string.IsNullOrEmpty(password) ||
               password.Length < 6)
                throw new InvalidOperationException(@"Invalid password, minimum 6 characters !");

            var user = (from su in _dataContext.Get<StreamingUser>()
                           where su.UserName == userName
                           select su).FirstOrDefault();
            if (user != null)
                return false;

            user = new StreamingUser();
            user.UserName = userName;
            user.PasswordHash = CryptographyHelper.ComputeHashSHA1($"{userName}_+_{password}{user.PasswordSalt}");
        
            _dataContext.Add(user);
            _dataContext.SaveChanges();
            return true;
        }

        /// <summary>
        /// Authentication of streaming user.
        /// </summary>
        /// <param name="userName">User name string</param>
        /// <param name="password">Password string.</param>
        /// <param name="hash">Hash string.</param>
        /// <param name="userId">Authenticated user identifier.</param>
        /// <returns></returns>
        public bool Authenticate(string userName, string password, string hash, out Guid userId)
        {
            userId = Guid.Empty;
            var user = (from su in _dataContext.Get<StreamingUser>()
                        where su.UserName == userName
                        select su).FirstOrDefault();
            if (user == null || user.IsActive == false)
                return false;

            var passwordHash = CryptographyHelper.ComputeHashSHA1($"{userName}_+_{password}{user.PasswordSalt}");
            if (user.PasswordHash == passwordHash)
            {
                userId = user.Id;
                return true;
            }
            return false;
        }
    }
}
