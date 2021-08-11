using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NorthernCrown.HttpRestClient
{
    public abstract class AbstractOAuthRestClient : AbstractRestClient
    {
        protected abstract string BearerToken { get; set; }
        protected abstract Uri AuthUri { get; }

        protected override HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            if (BearerToken != string.Empty)
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {BearerToken}");
            }

            return client;
        }

        protected abstract Task<bool> Authenticate();
        public async Task<string> Reauthorize()
        {
            BearerToken = string.Empty;
            await Authenticate();
            return this.BearerToken;
        }

        
    }
}
