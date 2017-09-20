using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace DialogAnalyzerFunc.Extensions
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Add headers to request
        /// </summary>
        public static void AddHeaders(this HttpRequestMessage request, IDictionary<string, string> headers)
        {
            // Add headers to request
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    request.Headers.Add(key, headers[key]);
                }
            }
        }

        /// <summary>
        /// Add content to request as byte array
        /// </summary>
        public static void AddContentAsBytes(this HttpRequestMessage request, byte[] content)
        {
            if (content?.Count() > 0)
            {
                ByteArrayContent byteContent = new ByteArrayContent(content);
                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                request.Content = byteContent;
            }
        }

        /// <summary>
        /// Add content to request as json
        /// </summary>
        public static void AddContentAsJson(this HttpRequestMessage request, object content)
        {
            if (content != null)
            {
                string jsonContent = JsonConvert.SerializeObject(content);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }
        }
    }
}
