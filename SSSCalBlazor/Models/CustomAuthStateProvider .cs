using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using System.Linq;
using System.Buffers.Text;
using Microsoft.IdentityModel.Tokens;
using static System.Net.WebRequestMethods;
using System.Net.Http.Json;

namespace SSSCalBlazor.Models
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private HttpClient client; 

        public CustomAuthStateProvider(ILocalStorageService localStorage, IHttpClientFactory _ClientFactory)
        {
            _localStorage = localStorage;
            client = _ClientFactory.CreateClient();

        }


        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _localStorage.GetItemAsStringAsync("accesstoken");
            string idtoken = await _localStorage.GetItemAsStringAsync("idtoken");

            //token = null;
            //await _localStorage.SetItemAsStringAsync("token", "");
            //await _localStorage.SetItemAsStringAsync("token",token);
            var identity = new ClaimsIdentity();
            client.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(idtoken))
            {
                try
                {
                    identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                }
                catch {
                    token = await HandleLogin();
                    if (token != null) 
                        identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                }
                if (token != null)
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        public class JwtToken
        {
            public long exp { get; set; }
            public List<string> roles { get; set; }
        }

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];

            JwtToken payload2 = JsonSerializer.Deserialize<JwtToken>(Base64UrlEncoder.Decode(payload));
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(payload2.exp);
            var dt= dateTimeOffset.LocalDateTime;
            var dff = DateTime.Now.Subtract(dt);

            var dd = dff.TotalSeconds; //>0 over expire time
            if (dd > 0) throw new Exception("token expired");


            var jsonBytes = ParseBase64WithoutPadding(payload);

            //var sver = System.Text.ASCIIEncoding.ASCII.GetString(jsonBytes);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            var nkeyval = new List<Claim>();
            foreach ( var kv in keyValuePairs) {
                var nkey = kv.Key;
                if (nkey == "username") nkey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
                nkeyval.Add(new Claim(nkey, kv.Value.ToString()));
            }
            if (payload2.roles != null && payload2.roles.Count>0)
            {
                foreach ( var kv in payload2.roles)
                    nkeyval.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", kv.ToString()));
            }

            return nkeyval;
//            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));

        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public class resTok
        {
            public string message { get; set; }
            public string tok { get; set; }
        }

        async Task<string> HandleLogin()
        {
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("reftoken");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                //var result = await client.PostAsync("http://localhost:3600/api/refreshtoken?token=" + token, null);
                var result = await client.PostAsync("https://www.schuebelsoftware.com/sso/api/refreshtoken?token=" + token, null);

                var tokestr = await result.Content.ReadAsStringAsync();
                if (tokestr == "Unauthorized") return null;

                var tokMsg = JsonSerializer.Deserialize<resTok>(tokestr);
                if (tokMsg == null) return null;
                if (tokMsg.message != "Post created...") return null;
                await _localStorage.SetItemAsStringAsync("accesstoken", tokMsg.tok);

                return tokMsg.tok ;
            }
            catch (Exception ex) { 
                var x=ex.Message;
            }

            await _localStorage.SetItemAsStringAsync("accesstoken", "");
            return null;
        }
    }
}
