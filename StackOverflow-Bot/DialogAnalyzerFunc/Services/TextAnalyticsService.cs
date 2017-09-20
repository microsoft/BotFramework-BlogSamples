using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DialogAnalyzerFunc.Models;
using DialogAnalyzerFunc.Utilities;

namespace DialogAnalyzerFunc.Services
{
    public class TextAnalyticsService
    {
        private readonly string HEADER_SUB_KEY = "Ocp-Apim-Subscription-Key";
        private readonly string SERVICE_URL_FORMAT = "https://{0}.api.cognitive.microsoft.com/text/analytics/v2.0/";

        public IDictionary<string, string> RequestHeaders { get; protected set; }

        public string BaseServiceUrl { get; protected set; }

        public TextAnalyticsService(string apiRegion, string subscriptionKey)
        {
            if (string.IsNullOrEmpty(apiRegion) == true)
            {
                throw new ArgumentNullException("Api region is not initialized.");
            }

            if (string.IsNullOrEmpty(subscriptionKey) == true)
            {
                throw new ArgumentNullException("Subscription key is not initialized.");
            }

            this.BaseServiceUrl = string.Format(SERVICE_URL_FORMAT, apiRegion);
            this.RequestHeaders = new Dictionary<string, string>()
            {
                {  this.HEADER_SUB_KEY, subscriptionKey }
            };
        }

        /// <summary>
        /// Analyze key phrases with text
        /// </summary>
        public async Task<TextAnalyticsResult<TextAnalyticsKeyPhrasesResult>> AnalyzeKeyPhrasesAsync(string fullText)
        {
            if (string.IsNullOrEmpty(fullText) == true)
            {
                throw new ArgumentNullException("Text is not initialized.");
            }

            // Get request uri
            Uri requestUri = new Uri(this.BaseServiceUrl + "keyPhrases");

            var document = new
            {
                id = Guid.NewGuid().ToString(),
                text = fullText
            };

            // Create content of the request
            var content = new
            {
                documents = new object[] { document }
            };

            // Get response
            return await HttpClientUtility.PostAsJsonAsync<TextAnalyticsResult<TextAnalyticsKeyPhrasesResult>>(requestUri, this.RequestHeaders, content);
        }
    }
}
