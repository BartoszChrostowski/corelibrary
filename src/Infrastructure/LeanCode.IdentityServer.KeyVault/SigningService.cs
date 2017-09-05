using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Tokens;

namespace LeanCode.IdentityServer.KeyVault
{
    using JsonWebKey = Microsoft.Azure.KeyVault.WebKey.JsonWebKey;

    class SigningService : IDisposable
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SigningService>();

        private readonly IdentityServerKeyVaultConfiguration config;
        private readonly KeyVaultClient keyVaultClient;

        private readonly SHA256 sha = SHA256.Create();

        private RsaSecurityKey key;

        public SigningService(IdentityServerKeyVaultConfiguration config)
        {
            this.config = config;

            keyVaultClient = new KeyVaultClient(GetToken);
        }

        public ValueTask<RsaSecurityKey> GetKeyAsync()
        {
            if (key != null)
            {
                return new ValueTask<RsaSecurityKey>(key);
            }
            return new ValueTask<RsaSecurityKey>(DownloadKeyAsync());
        }

        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var key = await GetKeyAsync();
            return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        }

        public async Task<string> SignTokenAsync(JwtSecurityToken jwt)
        {
            var hp = jwt.EncodedHeader + "." + jwt.EncodedPayload;

            var rawBytes = Encoding.UTF8.GetBytes(hp);
            var digest = sha.ComputeHash(rawBytes);

            var signatureRaw = await keyVaultClient
                .SignAsync(config.KeyId, JsonWebKeySignatureAlgorithm.RS256, digest);
            var signature = Base64Url.Encode(signatureRaw.Result);

            return hp + "." + signature;
        }

        public void Dispose()
        {
            keyVaultClient.Dispose();
            sha.Dispose();
        }

        private async Task<RsaSecurityKey> DownloadKeyAsync()
        {
            logger.Information("Downloading signing key");
            try
            {
                var keyResult = await keyVaultClient.GetKeyAsync(config.KeyId);
                key = new RsaSecurityKey(keyResult.Key.ToRSA());
                return key;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Cannot get signing key");
                throw;
            }
        }

        private async Task<string> GetToken(
            string authority, string resource, string scope)
        {
            try
            {
                var authCtx = new AuthenticationContext(authority);
                var cc = new ClientCredential(config.ClientId, config.ClientSecret);
                var token = await authCtx.AcquireTokenAsync(resource, cc);
                return token.AccessToken;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Cannot acqure access token from Azure AD");
                throw ex;
            }
        }
    }
}