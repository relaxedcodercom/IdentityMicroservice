﻿using IdentityMicroservice.BusinessLogic.Contracts;
using IdentityMicroservice.Domain.Entities;
using IdentityMicroservice.Repositories.Contracts;
using IdentityMicroservice.Services.Contracts;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace IdentityMicroservice.BusinessLogic
{
    public class AuthBusinessLogic : IAuthBusinessLogic
    {
        private readonly IUsersRepository usersRepository;
        private readonly IUserSessionsRepository userSessionsRepository;
        private readonly ICryptoService cryptoService;
        private readonly ITokenService tokenService;
        private readonly IConfiguration configuration;

        public AuthBusinessLogic(IUsersRepository usersRepository, IUserSessionsRepository userSessionsRepository,
            ICryptoService cryptoService, ITokenService tokenService, IConfiguration configuration)
        {
            this.usersRepository = usersRepository;
            this.userSessionsRepository = userSessionsRepository;
            this.cryptoService = cryptoService;
            this.tokenService = tokenService;
            this.configuration = configuration;
        }

        public async Task<User> ProcessRegisterUser(RegisterUser registerUser)
        {
            if (registerUser == null)
            {
                throw new ArgumentNullException(nameof(registerUser));
            }

            if (await usersRepository.UsernameExists(registerUser.Username))
            {
                throw new ApplicationException($"Username {registerUser.Username} is already used!");
            }

            if (await usersRepository.EmailExists(registerUser.Email))
            {
                throw new ApplicationException($"Email {registerUser.Email} is already used!");
            }

            return await usersRepository.Add(GetUserFromRegisterUser(registerUser));
        }

        public async Task<User> ProcessAuthenticate(LoginCredentials loginCredentials)
        {
            if (loginCredentials == null)
            {
                throw new ArgumentNullException(nameof(loginCredentials));
            }

            User user = null;
            if (await usersRepository.UsernameExists(loginCredentials.Username))
            {
                user = await usersRepository.GetByUsernameWithPassword(loginCredentials.Username);
            }
            else if (await usersRepository.EmailExists(loginCredentials.Username))
            {
                user = await usersRepository.GetByEmailWithPassword(loginCredentials.Username);
            }

            if (user == null || !cryptoService.VerifyPasswordHash(user.PasswordHash, loginCredentials.Password))
            {
                throw new ApplicationException("Incorrect username/email or password!");
            }

            user.PasswordHash = null;

            var usersClaims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            user.Token = tokenService.GenerateAccessToken(usersClaims);
            user.RefreshToken = tokenService.GenerateRefreshToken();

            await userSessionsRepository.DeleteExpiredByUserId(user.UserId);
            await userSessionsRepository.Add(GetUserSession(user.UserId, user.RefreshToken, loginCredentials.IpAddress));

            return user;
        }

        public async Task<TokenModel> ProcessRefreshToken(TokenModel tokenModel)
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(tokenModel.Token);
            var userId = new Guid(principal.Claims.First(a => a.Type == ClaimTypes.NameIdentifier).Value);

            if (!await userSessionsRepository.IsRefreshTokenValidForUser(tokenModel.RefreshToken, userId))
            {
                throw new ApplicationException("Refresh token is invalid!");
            }

            var result = new TokenModel();
            result.Token = tokenService.GenerateAccessToken(principal.Claims);
            result.RefreshToken = tokenService.GenerateRefreshToken();

            await userSessionsRepository.Add(GetUserSession(userId, result.RefreshToken, tokenModel.IpAddress));

            await userSessionsRepository.DeleteByRefreshTokenAndUser(tokenModel.RefreshToken, userId);

            return result;
        }

        public async Task ProcessLogout(string refreshToken, Guid userId)
        {
            await userSessionsRepository.DeleteByRefreshTokenAndUser(refreshToken, userId);
        }

        public async Task ProcessLogoutEverywhere(Guid userId)
        {
            await userSessionsRepository.DeleteByUserId(userId);
        }

        private UserSession GetUserSession(Guid userId, string refreshToken, string ipAddress)
        {
            return new UserSession
            {
                SessionId = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken,
                ExpirationDate = DateTime.UtcNow.AddHours(configuration.GetSection("Authentication:RefreshToken:DurationInHours").Get<int>()),
                IpAddress = ipAddress
            };
        }

        private User GetUserFromRegisterUser(RegisterUser registerUser)
        {
            return new User
            {
                UserId = Guid.NewGuid(),
                Username = registerUser.Username,
                Email = registerUser.Email,
                PasswordHash = cryptoService.GetPasswordHash(registerUser.Password)
            };
        }
    }
}