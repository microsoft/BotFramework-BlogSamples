using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DialogAnalyzerFunc.Models;
using DialogAnalyzerFunc.Utilities;

namespace DialogAnalyzerFunc.Services
{
    public class ComputerVisionService
    {
        private readonly string HEADER_OPLOC_KEY = "Operation-Location";
        private readonly string HEADER_SUB_KEY = "Ocp-Apim-Subscription-Key";
        private readonly string SERVICE_URL_FORMAT = "https://{0}.api.cognitive.microsoft.com/vision/v1.0/";

        private string AnalyzeImageVisualFeatures = "Description";

        public IDictionary<string, string> RequestHeaders { get; protected set; }

        public string BaseServiceUrl { get; protected set; }

        public ComputerVisionService(string apiRegion, string subscriptionKey)
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
        /// Analyze with image data
        /// </summary>
        public async Task<ComputerVisionImageAnalysisResult> AnalyzeImageAsync(byte[] imageData)
        {
            if (imageData?.Count() > 0 == false)
            {
                throw new ArgumentNullException("Image data is not initialized.");
            }

            // Get request uri
            Uri requestUri = new Uri(this.BaseServiceUrl + "analyze"
                                        + "?visualFeatures=" + this.AnalyzeImageVisualFeatures);

            // Get response
            return await HttpClientUtility.PostAsBytesAsync<ComputerVisionImageAnalysisResult>(requestUri, this.RequestHeaders, imageData);
        }

        /// <summary>
        /// Analyze with image uri
        /// </summary>
        public async Task<ComputerVisionImageAnalysisResult> AnalyzeImageAsync(Uri imageUri)
        {
            string url = imageUri?.AbsoluteUri ?? throw new ArgumentNullException("Image uri is not initialized.");

            // Get request uri
            Uri requestUri = new Uri(this.BaseServiceUrl + "analyze"
                                        + "?visualFeatures=" + this.AnalyzeImageVisualFeatures);

            // Create content of the request
            var content = new { Url = url };

            // Get response
            return await HttpClientUtility.PostAsJsonAsync<ComputerVisionImageAnalysisResult>(requestUri, this.RequestHeaders, content);
        }

        /// <summary>
        /// Recognize handwritten text with image data
        /// </summary>
        public async Task<HandwritingRecognitionResult> RecognizeHandwrittenTextAsync(byte[] imageData)
        {
            if (imageData?.Count() > 0 == false)
            {
                throw new ArgumentNullException("Image data is not initialized.");
            }

            // Get request uri
            Uri requestUri = new Uri(this.BaseServiceUrl + "recognizeText?handwriting=true");

            // Get response
            HttpResponseMessage response = await HttpClientUtility.PostAsBytesAsync(requestUri, this.RequestHeaders, imageData);

            return await GetResultFromOperationResponse(response);
        }

        /// <summary>
        /// Recognize handwritten text with image uri
        /// </summary>
        public async Task<HandwritingRecognitionResult> RecognizeHandwrittenTextAsync(Uri imageUri)
        {
            string url = imageUri?.AbsoluteUri ?? throw new ArgumentNullException("Image uri is not initialized.");

            // Get request uri
            Uri requestUri = new Uri(this.BaseServiceUrl + "recognizeText?handwriting=true");

            // Create content of the request
            var content = new { Url = url };

            // Get response
            HttpResponseMessage response = await HttpClientUtility.PostAsJsonAsync(requestUri, this.RequestHeaders, content);

            return await GetResultFromOperationResponse(response);
        }

        private async Task<HandwritingRecognitionResult> GetResultFromOperationResponse(HttpResponseMessage response)
        {
            // Process operation
            if (response.Headers.Contains(this.HEADER_OPLOC_KEY) == false)
            {
                throw new InvalidOperationException("No operation-location value returned from initial request.");
            }

            Uri opLocationUri = new Uri(response.Headers.GetValues(this.HEADER_OPLOC_KEY).First());

            HandwritingRecognitionOperationResult opResult = new HandwritingRecognitionOperationResult();

            int i = 0;
            while (i++ < HttpClientUtility.RETRY_COUNT)
            {
                // Get the operation result
                opResult = await HttpClientUtility.GetAsync<HandwritingRecognitionOperationResult>(opLocationUri, this.RequestHeaders);

                // Wait if operation is running or has not started
                if (opResult.Status == HandwritingRecognitionOperationResult.HandwritingRecognitionOperationStatus.NotStarted
                    || opResult.Status == HandwritingRecognitionOperationResult.HandwritingRecognitionOperationStatus.Running)
                {
                    await Task.Delay(HttpClientUtility.RETRY_DELAY);
                }
                else
                {
                    break;
                }
            }

            if (opResult.Status != HandwritingRecognitionOperationResult.HandwritingRecognitionOperationStatus.Succeeded)
            {
                throw new Exception($"Handwriting recognition operation was not successful with status: {opResult.Status}");
            }

            return opResult.Result;
        }
    }
}
