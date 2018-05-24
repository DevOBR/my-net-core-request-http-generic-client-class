

namespace RequestClient
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RequesClient : IDisposable
    {
        private HttpClient client;
        private string baseAddress;
        private User user;

        public RequesClient(string baseAddress, string userName, string pwd)
        {
            this.baseAddress = baseAddress;
            this.user = new User
            {
                UserName = userName,
                Password = pwd
            };
            this.client = new HttpClient();
            this.client.BaseAddress = new Uri(baseAddress);
        }

        public async Task<string> GetWithBasicRequest(string urlApi)
        {
            string userName = string.Empty;
            string pwd = string.Empty;

            if (user != null)
            {
                userName = this.user.UserName;
                pwd = this.user.Password;
            }

            if (client != null)
            {
                this.client.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", userName, pwd))));

                HttpResponseMessage response = await this.client.GetAsync(urlApi);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        public async Task<string> GetWithBearerToken(string urltoken, string urlApi)
        {
            string userName = string.Empty;
            string pwd = string.Empty;

            if (user != null)
            {
                userName = this.user.UserName;
                pwd = this.user.Password;
            }

            var tokenResult = await this.GetToken(urltoken, userName, pwd);
            string token = string.Empty;

            if (tokenResult != null)
            {
                var validate = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenResult);
                if (validate != null && validate.Any())
                    token = validate.Where(type => type.Key == "access_token").FirstOrDefault().Value;
            }

            if (client != null)
            {
                this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await this.client.GetAsync(urlApi);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            }

            return null;

        }


        public Task<string> GetToken(string urlToken, string userName, string pwd)
        {

            var login = new Dictionary<string, string>
            {
                {"grant_type", "password"},
                {"username", userName},
                {"password", pwd},
            };

            var response = this.client
                .PostAsync(urlToken, new FormUrlEncodedContent(login)).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync();

            return null;

        }

        public void Dispose()
        {
            this.client?.Dispose();
        }
    }
}
