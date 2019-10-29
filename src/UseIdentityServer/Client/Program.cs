using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // step 1. discovery document
            // var disco = Task.Run(() => client.GetDiscoveryDocumentAsync("http://localhost:5000")).Result;
            // if (disco.IsError)
            // {
            //     Console.WriteLine(disco.Error);
            // }

            // step 2. get token use endpoint
            // var tokenResponse = Task.Run(() => client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            // {
            //     Address = disco.TokenEndpoint,
            //     ClientId = "client",
            //     ClientSecret = "secret",
            //     Scope = "api1"
            // })).Result;
            // if (tokenResponse.IsError)
            // {
            //     Console.WriteLine(tokenResponse.Error);
            //     return;
            // }

            // step 3. call api use token
            // client.SetBearerToken(tokenResponse.AccessToken);
            // var response = Task.Run(() => client.GetAsync("http://localhost:5001/identity")).Result;
            // if (!response.IsSuccessStatusCode)
            // {
            //     Console.WriteLine(response.StatusCode);
            // }
            // else
            // {
            //     var content = response.Content.ReadAsStringAsync().Result;
            //     Console.WriteLine(JArray.Parse(content));
            // }

            var tokenEndpoint = Task.Run(() => GetTokenEndpointAsync()).Result;

            // var accessToken = Task.Run(() => GetTokenByClientCredentialsAsync(tokenEndpoint)).Result;
            var accessToken = Task.Run(() => GetTokenByPasswordAsync(tokenEndpoint)).Result;

            var content = Task.Run(() => CallApiAsync(accessToken)).Result;
        }

        // private static string methodName = MethodBase.GetCurrentMethod().Name;
        private static HttpClient client = new HttpClient();

        private static async Task<string> GetTokenEndpointAsync(string url = "http://localhost:5000")
        {
            var response = await client.GetDiscoveryDocumentAsync(url);
            if (response.IsError)
            {
                Console.WriteLine($"{nameof(GetTokenEndpointAsync)} ~ error: {response.Error}");
                return string.Empty;
            }

            Console.WriteLine($"{nameof(GetTokenEndpointAsync)} ~ token_endpoint: {response.TokenEndpoint}");
            return response.TokenEndpoint;
        }

        private static async Task<string> GetTokenByClientCredentialsAsync(
            string tokenEndpoint = "http://localhost:5000/connect/token",
            string clientId = "client",
            string clientSecret = "secret",
            string scope = "api1")
        {
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope
            });

            if (response.IsError)
            {
                Console.WriteLine($"{nameof(GetTokenByClientCredentialsAsync)} ~ error: {response.Error}");
                return string.Empty;
            }

            Console.WriteLine($"{nameof(GetTokenByClientCredentialsAsync)} ~ result: {response.AccessToken}");
            return response.AccessToken;
        }

        private static async Task<string> GetTokenByPasswordAsync(
            string tokenEndpoint = "http://localhost:5000/connect/token",
            string clientId = "ro.client",
            string clientSecret = "secret",
            string scope = "api1",
            string userName = "alice",
            string password = "123456")
        {
            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope,
                UserName = userName,
                Password = password
            });

            if (response.IsError)
            {
                Console.WriteLine($"{nameof(GetTokenByClientCredentialsAsync)} ~ error: {response.Error}");
                return string.Empty;
            }

            Console.WriteLine($"{nameof(GetTokenByClientCredentialsAsync)} ~ result: {response.AccessToken}");
            return response.AccessToken;
        }

        private static async Task<string> CallApiAsync(string accessToken, string url = "http://localhost:5001/identity")
        {
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{nameof(CallApiAsync)} ~ code: {response.StatusCode}");
                return string.Empty;
            }

            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"{nameof(CallApiAsync)} ~ content: {JArray.Parse(content)}");
            return content;
        }
    }
}
