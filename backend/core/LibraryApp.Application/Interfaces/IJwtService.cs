using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace LibraryApp.Application.Interfaces
{
    /// <summary>
    /// JWT üretim ve doğrulama servis arayüzü.
    /// Nested JWS → JWE (asimetrik) ile Access / Refresh token desteği.
    /// </summary>
    public interface IJwtService 
    {
        /// <summary>
        /// Access token (kısa ömürlü, nested JWS→JWE) üretir.
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="email">Kullanıcı email</param>
        /// <param name="role">Kullanıcı rolü</param>
        /// <param name="extraClaims">Ek payload alanları (opsiyonel)</param>
        /// <returns>JWE (5 parçalı) access token string</returns>
        string CreateAccessToken(int userId, string email, string role, IDictionary<string, object>? extraClaims = null);

        /// <summary>
        /// Refresh token (uzun ömürlü, nested JWS→JWE) üretir.
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="deviceId">Opsiyonel cihaz/oturum tanımlayıcısı</param>
        /// <returns>JWE (5 parçalı) refresh token string</returns>
        string CreateRefreshToken(int userId, string deviceId = "");

        /// <summary>
        /// Access token doğrulama (JWE decrypt + JWS verify).
        /// Geçerliyse ClaimsPrincipal döner, yoksa SecurityTokenException fırlatır.
        /// </summary>
        /// <param name="jweToken">JWE token string</param>
        Task<ClaimsPrincipal> ValidateAccessToken(string jweToken);

        /// <summary>
        /// Refresh token doğrulama (JWE decrypt + JWS verify).
        /// Geçerliyse ClaimsPrincipal döner, yoksa null döner.
        /// </summary>
        /// <param name="jweToken">JWE refresh token string</param>
        Task<ClaimsPrincipal?> ValidateRefreshToken(string jweToken);

        /// <summary>
        /// Access ve Refresh token'ları HttpOnly+Secure cookie olarak yazar.
        /// </summary>
        /// <param name="accessJwe">Access token</param>
        /// <param name="refreshJwe">Refresh token</param>
        void SetAuthCookies(string accessJwe, string refreshJwe);

        /// <summary>
        /// Sadece Access token'ı HttpOnly+Secure cookie olarak yazar (refresh için).
        /// </summary>
        /// <param name="accessJwe">Access token</param>
        void SetAccessCookie(string accessJwe);

        /// <summary>
        /// Access ve Refresh cookie'lerini temizler (expire geçmişe alır).
        /// </summary>
        void ClearAuthCookies();

        /// <summary>
        /// Access token ömrü.
        /// </summary>
        TimeSpan AccessLifetime { get; }

        /// <summary>
        /// Refresh token ömrü.
        /// </summary>
        TimeSpan RefreshLifetime { get; }
    }
}
