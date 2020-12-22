using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NorthernCrown.HttpRestClient
{
    public abstract class AbstractRestClient : IRestClient
    {
        protected abstract Uri RootUrl { get; }

        private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
        {
            using (var client = GetHttpClient())
            {
                var response = await client.SendAsync(request);
                return response;
            }
        }

        private async Task<string> SendRequestWithBody(HttpMethod method, string url, object content, string contentType)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = GetUri(url),
                Content = GetHttpContent(content, contentType)
            };
            var response = await SendRequest(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> Delete(string url)
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = GetUri(url) };
            var response = await SendRequest(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> Get(string url)
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = GetUri(url) };
            var response = await SendRequest(request);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public async Task<string> Patch(string url, object body, string contentType = "application/json")
        {
            return await SendRequestWithBody(HttpMethod.Patch, url, body, contentType);
        }

        public async Task<string> Post(string url, object body, string contentType = "application/json")
        {
            return await SendRequestWithBody(HttpMethod.Post, url, body, contentType);
        }

        public async Task<string> Put(string url, object body, string contentType = "application/json")
        {
            return await SendRequestWithBody(HttpMethod.Put, url, body, contentType);
        }

        protected HttpContent GetHttpContent(object body, string contentType)
        {
            if (contentType == "application/json")
            {
                return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, contentType);
            }
            else if (contentType == "application/x-www-form-urlencoded")
            {
                //for forms, content needs to be in the form of a dictionary
                var d = (Dictionary<string, string>)body;
                return new FormUrlEncodedContent(d);
            }
            else
            {
                throw new Exception($"Invalid content type: {contentType}");
            }
        }

        protected abstract HttpClient GetHttpClient();
        protected Uri GetUri(string path)
        {
            return new Uri(RootUrl, path);
        }
    }
}
