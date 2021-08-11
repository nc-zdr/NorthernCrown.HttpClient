using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

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

        private async Task<string> SendRequestWithBody(HttpMethod method, string url, object content, string contentType, int trycount = 0)
        {
            try
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
            catch (System.Net.Http.HttpRequestException x)
            {
                if (trycount >= 5)
                    throw new Exception("Bailing, server isn't responding");

                trycount++;
                Console.WriteLine("Hm, had a timeout. Probably made the server mad. Let's take a break");
                Thread.Sleep(30000);
                return await SendRequestWithBody(method, url, content, contentType, trycount);
            }
        }

        public async Task<bool> Delete(string url)
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = GetUri(url) };
            var response = await SendRequest(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> Get(string url, int trycount = 0)
        {
            try
            {
                var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = GetUri(url) };
                var response = await SendRequest(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    Console.WriteLine(response.ReasonPhrase);

                    if (trycount >= 5)
                        throw new Exception("Okay, something's broken. I quit.");

                    trycount++;
                    Console.WriteLine("Didn't get a 200 in response, waiting, trying again");
                    Thread.Sleep(30000);
                    return await Get(url, trycount);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    return content;
                }
            }
            catch (System.Net.Http.HttpRequestException x)
            {
                if (trycount >= 5)
                    throw new Exception("Bailing, server isn't responding");

                trycount++;
                Console.WriteLine("Hm, had a timeout. Probably made the server mad. Let's take a break");
                Thread.Sleep(30000);
                return await Get(url, trycount);
            }
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
                return new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, contentType);
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
