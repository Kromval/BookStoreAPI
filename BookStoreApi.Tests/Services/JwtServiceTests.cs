//using System;
//using System.Linq;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using Xunit;
//using BookStoreApi.Models;
//using BookStoreApi.Enums;

//namespace BookStoreApi.Tests.Services
//{
//    public class JwtServiceTests
//    {
//        private readonly JwtService _jwtService;
//        private readonly IConfiguration _configuration;

//        public JwtServiceTests()
//        {
//            // Подменяем IConfiguration
//            var inMemorySettings = new[]
//            {
//                new KeyValuePair<string, string>("Jwt:Key", "MyTestSuperSecretKey123!"),
//                new KeyValuePair<string, string>("Jwt:Issuer", "TestIssuer"),
//                new KeyValuePair<string, string>("Jwt:Audience", "TestAudience")
//            };
//            _configuration = new ConfigurationBuilder()
//                .AddInMemoryCollection(inMemorySettings)
//                .Build();

//            _jwtService = new JwtService(_configuration);
//        }

//        [Fact]
//        public void GenerateToken_ShouldReturn_NonEmptyString()
//        {
//            // Arrange: добавили PasswordHash
//            var user = new User
//            {
//                Username = "testuser",
//                Role = UserRole.Admin,
//                PasswordHash = "dummyHash"
//            };

//            // Act
//            var token = _jwtService.GenerateToken(user);

//            // Assert
//            Assert.False(string.IsNullOrEmpty(token));
//            Assert.Contains('.', token); // JWT из трёх частей
//        }

//        [Fact]
//        public void GenerateToken_ShouldContain_CorrectClaimsIssuerAndAudience()
//        {
//            // Arrange
//            var user = new User
//            {
//                Username = "alice",
//                Role = UserRole.User,
//                PasswordHash = "dummyHash"
//            };

//            // Act
//            var tokenString = _jwtService.GenerateToken(user);

//            // Decode без проверки подписи
//            var handler = new JwtSecurityTokenHandler();
//            var jwtToken = handler.ReadJwtToken(tokenString);

//            // Assert: Claims
//            Assert.Equal("alice", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
//            Assert.Equal(UserRole.User.ToString(), jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);

//            // Assert: issuer/audience
//            Assert.Equal("TestIssuer", jwtToken.Issuer);
//            Assert.Equal("TestAudience", jwtToken.Audiences.First());
//        }

//        [Fact]
//        public void GenerateToken_ShouldHave_ValidExpiration()
//        {
//            // Arrange
//            var user = new User
//            {
//                Username = "bob",
//                Role = UserRole.User,
//                PasswordHash = "dummyHash"
//            };
//            var before = DateTime.UtcNow;

//            // Act
//            var tokenString = _jwtService.GenerateToken(user);

//            // Decode
//            var handler = new JwtSecurityTokenHandler();
//            var jwtToken = handler.ReadJwtToken(tokenString);

//            // Assert: expires примерно через 12 часов
//            var validTo = jwtToken.ValidTo;
//            Assert.True(validTo > before.AddHours(11));
//            Assert.True(validTo < before.AddHours(13));
//        }

//        [Fact]
//        public void GeneratedToken_ShouldBe_ProperlySigned_HmacSha256()
//        {
//            // Arrange
//            var user = new User
//            {
//                Username = "charlie",
//                Role = UserRole.User,
//                PasswordHash = "dummyHash"
//            };

//            // Act
//            var tokenString = _jwtService.GenerateToken(user);

//            // Объявляем handler
//            var handler = new JwtSecurityTokenHandler();

//            // Настраиваем валидацию подписи
//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
//            var tokenParams = new TokenValidationParameters
//            {
//                ValidateIssuer = true,
//                ValidIssuer = _configuration["Jwt:Issuer"],
//                ValidateAudience = true,
//                ValidAudience = _configuration["Jwt:Audience"],
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKey = key,
//                ValidateLifetime = false  // проверяем только подпись
//            };

//            // Assert: валидация не должна выбросить исключений
//            handler.ValidateToken(tokenString, tokenParams, out var validatedToken);
//            Assert.IsType<JwtSecurityToken>(validatedToken);
//            Assert.Equal(SecurityAlgorithms.HmacSha256, ((JwtSecurityToken)validatedToken).Header.Alg);
//        }
//    }
//}
