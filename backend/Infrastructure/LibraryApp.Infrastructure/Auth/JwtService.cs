using System;
using System.Collections.Generic;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.Infrastructure.Auth
{

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _cfg;
        private readonly IHttpContextAccessor? _http;

        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _sigPrivatePath;
        private readonly string _sigPublicPath;
        private readonly string _encPublicPath;
        private readonly string _encPrivatePath;
        private readonly string _sigKid;
        private readonly string _encKid;
        private readonly string _accessCookieName;
        private readonly string _refreshCookieName;
        public TimeSpan AccessLifetime { get; }
        public TimeSpan RefreshLifetime { get; }

        public JwtService(IConfiguration cfg, IHttpContextAccessor? httpContextAccessor = null)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _http = httpContextAccessor;

            _issuer = _cfg["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
            _audience = _cfg["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
            _sigPrivatePath = _cfg["Jwt:SigPrivatePath"] ?? throw new ArgumentNullException("Jwt:SigPrivatePath");
            _sigPublicPath = _cfg["Jwt:SigPublicPath"] ?? throw new ArgumentNullException("Jwt:SigPublicPath");
            _encPublicPath = _cfg["Jwt:EncPublicPath"] ?? throw new ArgumentNullException("Jwt:EncPublicPath");
            _encPrivatePath = _cfg["Jwt:EncPrivatePath"] ?? throw new ArgumentNullException("Jwt:EncPrivatePath");
            _sigKid = _cfg["Jwt:SigKid"] ?? "sig-default";
            _encKid = _cfg["Jwt:EncKid"] ?? "enc-default";
            _accessCookieName = _cfg["Jwt:AccessCookieName"] ?? "access";
            _refreshCookieName = _cfg["Jwt:RefreshCookieName"] ?? "refresh";

            if (!int.TryParse(_cfg["Jwt:AccessMinutes"], out var accessMin)) accessMin = 10;
            if (!int.TryParse(_cfg["Jwt:RefreshDays"], out var refreshDays)) refreshDays = 14;

            AccessLifetime = TimeSpan.FromMinutes(accessMin);
            RefreshLifetime = TimeSpan.FromDays(refreshDays);
        }

        // -------------------- PUBLIC API --------------------

        public string CreateAccessToken(int userId, string email, string role, IDictionary<string, object>? extraClaims = null)
        {
            var now = DateTime.UtcNow;

            // İç JWS için imza anahtarı (RS256)
            var rsaSign = LoadRsaPrivateKey(_sigPrivatePath);
            var signKey = new RsaSecurityKey(rsaSign) { KeyId = _sigKid };
            var signCreds = new SigningCredentials(signKey, SecurityAlgorithms.RsaSha256);

            // Dış JWE için şifreleme anahtarı (RSA-OAEP + A256CBC-HS512)
            var rsaEncPub = LoadRsaPublicKey(_encPublicPath);
            var encKey = new RsaSecurityKey(rsaEncPub) { KeyId = _encKid };
            var encCreds = new EncryptingCredentials(
                encKey,
                SecurityAlgorithms.RsaOAEP,      // OAEP (SHA-1) yaygın destekli
                SecurityAlgorithms.Aes256CbcHmacSha512    // A256CBC-HS512
            );

            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat, Epoch(now).ToString(), ClaimValueTypes.Integer64)
            });

            if (extraClaims != null)
            {
                foreach (var kv in extraClaims)
                {
                    // JWT payload'ı nesne taşıyabilir; basit string tipine indirgemek isterseniz ToString() kullanın
                    claimsIdentity.AddClaim(new Claim(kv.Key, kv.Value?.ToString() ?? ""));
                }
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                Subject = claimsIdentity,
                Expires = now.Add(AccessLifetime),
                SigningCredentials = signCreds,      // Inner JWS
                EncryptingCredentials = encCreds     // Outer JWE
            };

            var handler = new JsonWebTokenHandler();
            string jwe = handler.CreateToken(descriptor); // 5 parçalı JWE (içte JWS)
            return jwe;
        }

        public string CreateRefreshToken(int userId, string deviceId = "")
        {
            var now = DateTime.UtcNow;

            var rsaSign = LoadRsaPrivateKey(_sigPrivatePath);
            var signKey = new RsaSecurityKey(rsaSign) { KeyId = _sigKid };
            var signCreds = new SigningCredentials(signKey, SecurityAlgorithms.RsaSha256);

            var rsaEncPub = LoadRsaPublicKey(_encPublicPath);
            var encKey = new RsaSecurityKey(rsaEncPub) { KeyId = _encKid };
            var encCreds = new EncryptingCredentials(encKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512);

            // Refresh: minimal claim + cihaz/oturum bilgisi + uzun süre
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("typ", "refresh"),
                new Claim("did", deviceId ?? string.Empty),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat, Epoch(now).ToString(), ClaimValueTypes.Integer64)
            });

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                Subject = claims,
                Expires = now.Add(RefreshLifetime),
                SigningCredentials = signCreds,
                EncryptingCredentials = encCreds
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }

        public async Task<ClaimsPrincipal> ValidateAccessToken(string jweToken)
        {
            if (string.IsNullOrWhiteSpace(jweToken))
                throw new SecurityTokenException("Empty token");

            // 1) JWE decrypt (private key)  2) inner JWS verify (public key)
            var rsaEncPriv = LoadRsaPrivateKey(_encPrivatePath);
            var decKey = new RsaSecurityKey(rsaEncPriv) { KeyId = _encKid };

            var rsaSignPub = LoadRsaPublicKey(_sigPublicPath);
            var sigKey = new RsaSecurityKey(rsaSignPub) { KeyId = _sigKid };

            var tvp = new TokenValidationParameters
            {
                TokenDecryptionKey = decKey,     // JWE decrypt
                IssuerSigningKey = sigKey,     // JWS verify
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var handler = new JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(jweToken, tvp);

            if (!result.IsValid)
                throw new SecurityTokenException(result.Exception?.Message ?? "Invalid token");

            // ClaimsIdentity → ClaimsPrincipal
            var identity = result.ClaimsIdentity ?? throw new SecurityTokenException("No identity");
            return new ClaimsPrincipal(identity);
        }

        public async Task<ClaimsPrincipal?> ValidateRefreshToken(string jweToken)
        {
            if (string.IsNullOrWhiteSpace(jweToken))
                return null;

            try
            {
                // 1) JWE decrypt (private key)  2) inner JWS verify (public key)
                var rsaEncPriv = LoadRsaPrivateKey(_encPrivatePath);
                var decKey = new RsaSecurityKey(rsaEncPriv) { KeyId = _encKid };

                var rsaSignPub = LoadRsaPublicKey(_sigPublicPath);
                var sigKey = new RsaSecurityKey(rsaSignPub) { KeyId = _sigKid };

                var tvp = new TokenValidationParameters
                {
                    TokenDecryptionKey = decKey,     // JWE decrypt
                    IssuerSigningKey = sigKey,     // JWS verify
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                var handler = new JsonWebTokenHandler();
                var result = await handler.ValidateTokenAsync(jweToken, tvp);

                if (!result.IsValid)
                    return null;

                // ClaimsIdentity → ClaimsPrincipal
                var identity = result.ClaimsIdentity;
                if (identity == null) return null;
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        public void SetAuthCookies(string accessJwe, string refreshJwe)
        {
            if (_http?.HttpContext == null) return;

            var ctx = _http.HttpContext;
            var now = DateTimeOffset.UtcNow;

            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // localhost için false
                SameSite = SameSiteMode.Lax, // CORS için Lax
                Expires = now.Add(AccessLifetime),
                IsEssential = true,
                Domain = null // localhost için null bırak
            };
            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // localhost için false
                SameSite = SameSiteMode.Lax, // CORS için Lax
                Expires = now.Add(RefreshLifetime),
                IsEssential = true,
                Domain = null // localhost için null bırak
            };

            ctx.Response.Cookies.Append(_accessCookieName, accessJwe, accessOptions);
            ctx.Response.Cookies.Append(_refreshCookieName, refreshJwe, refreshOptions);

            // Debug log
            Console.WriteLine($"Cookies set - Access: {_accessCookieName}={accessJwe.Substring(0, Math.Min(20, accessJwe.Length))}..., Refresh: {_refreshCookieName}={refreshJwe.Substring(0, Math.Min(20, refreshJwe.Length))}...");
            Console.WriteLine($"Access cookie expires: {accessOptions.Expires}, Refresh cookie expires: {refreshOptions.Expires}");
        }

        public void SetAccessCookie(string accessJwe)
        {
            if (_http?.HttpContext == null) return;

            var ctx = _http.HttpContext;
            var now = DateTimeOffset.UtcNow;

            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // localhost için false
                SameSite = SameSiteMode.Lax, // CORS için Lax
                Expires = now.Add(AccessLifetime),
                IsEssential = true,
                Domain = null // localhost için null bırak
            };

            ctx.Response.Cookies.Append(_accessCookieName, accessJwe, accessOptions);
        }

        public void ClearAuthCookies()
        {
            if (_http?.HttpContext == null) return;
            var ctx = _http.HttpContext;

            var opts = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // localhost için false
                SameSite = SameSiteMode.Lax, // CORS için Lax
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                Domain = null // localhost için null bırak
            };

            ctx.Response.Cookies.Append(_accessCookieName, "", opts);
            ctx.Response.Cookies.Append(_refreshCookieName, "", opts);
        }

        // -------------------- HELPERS --------------------

        private static long Epoch(DateTime utc) =>
            new DateTimeOffset(utc).ToUnixTimeSeconds();

        // RSA key'leri cache'lemek için
        private static readonly Dictionary<string, RSA> _rsaCache = new();
        private static readonly object _rsaLock = new();

        private static RSA LoadRsaPrivateKey(string path)
        {
            lock (_rsaLock)
            {
                if (_rsaCache.TryGetValue(path, out var cachedRsa))
                {
                    return cachedRsa;
                }

                var bytes = File.ReadAllBytes(path);
                var rsa = RSA.Create();

                if (LooksLikePem(bytes))
                {
                    var pem = Encoding.ASCII.GetString(bytes);
                    rsa.ImportFromPem(pem.ToCharArray());
                }
                else
                {
                    // DER (PKCS#8 veya PKCS#1 private)
                    TryImportPrivateDer(rsa, bytes);
                }

                _rsaCache[path] = rsa;
                return rsa;
            }
        }

        private static RSA LoadRsaPublicKey(string path)
        {
            lock (_rsaLock)
            {
                if (_rsaCache.TryGetValue(path, out var cachedRsa))
                {
                    return cachedRsa;
                }

                var bytes = File.ReadAllBytes(path);
                var rsa = RSA.Create();

                if (LooksLikePem(bytes))
                {
                    var pem = Encoding.ASCII.GetString(bytes);
                    rsa.ImportFromPem(pem.ToCharArray());
                }
                else
                {
                    // DER public
                    rsa.ImportSubjectPublicKeyInfo(bytes, out _);
                }

                _rsaCache[path] = rsa;
                return rsa;
            }
        }

        private static bool LooksLikePem(byte[] bytes)
        {
            // basit kontrol
            var s = Encoding.ASCII.GetString(bytes);
            return s.Contains("BEGIN ") && s.Contains("END ");
        }

        private static void TryImportPrivateDer(RSA rsa, byte[] der)
        {
            // Sırasıyla dene: PKCS#8 → PKCS#1
            try
            {
                rsa.ImportPkcs8PrivateKey(der, out _);
            }
            catch
            {
                rsa.ImportRSAPrivateKey(der, out _);
            }
        }
    }
}
