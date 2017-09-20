using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using DialogAnalyzerFunc.Extensions;

namespace DialogAnalyzerFunc.Utilities
{
    public static class HttpClientUtility
    {
        public static readonly int RETRY_COUNT = 10;
        public static readonly int RETRY_DELAY = 500;

        private static HttpClient Client;

        /// <summary>
        /// Static constructor of the HttpClientUtility
        /// </summary>
        static HttpClientUtility()
        {
            if (Client == null)
            {
                Client = new HttpClient();
            }
        }

        /// <summary>
        /// Send Http Get to the request uri and get the TResult from response content
        /// </summary>
        public static async Task<TResult> GetAsync<TResult>(Uri requestUri, IDictionary<string, string> headers)
        {
            // Get response
            HttpResponseMessage response = await GetAsync(requestUri, headers);

            // Read response
            string responseContent = await response.Content.ReadAsStringAsync();

            // Get result
            TResult result = JsonConvert.DeserializeObject<TResult>(responseContent);
            return result;
        }

        /// <summary>
        /// Send Http Get to the request uri and get HttpResponseMessage
        /// </summary>
        public static async Task<HttpResponseMessage> GetAsync(Uri requestUri, IDictionary<string, string> headers)
        {
            // Create new request function
            Func<HttpRequestMessage> createRequestMessage = () =>
            {
                // Create new request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                // Add headers to request
                request.AddHeaders(headers);
                return request;
            };

            // Send request and get response
            HttpResponseMessage response = await ExecuteActionkWithAutoRetry(() => Client.SendAsync(createRequestMessage()));
            return response;
        }

        /// <summary>
        /// Send Http Post to request uri and get TResult from response content 
        /// </summary>
        public static async Task<TResult> PostAsBytesAsync<TResult>(Uri requestUri, IDictionary<string, string> headers, byte[] content)
        {
            // Post request and get response
            HttpResponseMessage response = await PostAsBytesAsync(requestUri, headers, content);

            // Read response
            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResult>(responseContent);
        }

        /// <summary>
        /// Send Http Post to request uri and get TResult from response content 
        /// </summary>
        public static async Task<TResult> PostAsJsonAsync<TResult>(Uri requestUri, IDictionary<string, string> headers, object content)
        {
            // Post request and get response
            HttpResponseMessage response = await PostAsJsonAsync(requestUri, headers, content);

            // Read response
            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResult>(responseContent);
        }

        /// <summary>
        /// Send Http Post to request uri and get HttpResponseMessage
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsBytesAsync(Uri requestUri, IDictionary<string, string> headers, byte[] content)
        {
            // Create new request function
            Func<HttpRequestMessage> createRequestMessage = () =>
            {
                // Create new request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

                // Add headers to request
                request.AddHeaders(headers);

                // Add content as Json
                request.AddContentAsBytes(content);

                return request;
            };

            // Post request
            HttpResponseMessage response = await ExecuteActionkWithAutoRetry(() => Client.SendAsync(createRequestMessage()));
            return response;
        }

        /// <summary>
        /// Send Http Post to request uri and get HttpResponseMessage
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsJsonAsync(Uri requestUri, IDictionary<string, string> headers, object content)
        {
            // Create new request function
            Func<HttpRequestMessage> createRequestMessage = () =>
            {
                // Create new request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

                // Add headers to request
                request.AddHeaders(headers);

                // Add content as Json
                request.AddContentAsJson(content);

                return request;
            };

            // Post request
            HttpResponseMessage response = await ExecuteActionkWithAutoRetry(() => Client.SendAsync(createRequestMessage()));
            return response;
        }

        /// <summary>
        /// Execute the action which returns HttpResponseMessage with auto retry
        /// </summary>
        private static async Task<HttpResponseMessage> ExecuteActionkWithAutoRetry(Func<Task<HttpResponseMessage>> action)
        {
            int retryCount = RETRY_COUNT;
            int retryDelay = RETRY_DELAY;

            HttpResponseMessage response;

            while (true)
            {
                response = await action();

                if (response.StatusCode == (HttpStatusCode)429 && retryCount > 0)
                {
                    await Task.Delay(retryDelay);
                    retryCount--;
                    retryDelay *= 2;
                    continue;
                }

                response.EnsureSuccessStatusCode();
                break;
            }

            return response;
        }

    }
}
