using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using DialogAnalyzerFunc.Clients;
using DialogAnalyzerFunc.Models;

namespace DialogAnalyzerFunc
{
    public static class AnalyzeDialog
    {
        private struct RequestBody
        {
            public Uri ImageUri { get; set; }
        }

        private static DialogAnalyzerClient client;

        private static DialogAnalyzerClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new DialogAnalyzerClient(
                        computerVisionApiRegion: ComputerVisionApiRegion,
                        computerVisionSubscriptionKey: ComputerVisionSubscriptionKey,
                        textAnalyticsApiRegion: TextAnalyticsApiRegion,
                        textAnalyticsSubscriptionKey: TextAnalyticsSubscriptionKey
                    );
                }
                return client;
            }
        }

        [FunctionName("AnalyzeDialog")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "AnalyzeDialog")]HttpRequestMessage request, TraceWriter log)
        {
            try
            {
                MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;

                // Check if content type is empty
                if (contentType == null)
                {
                    return request.CreateResponse(HttpStatusCode.BadRequest, "Missing content-type from header.");
                }

                // Check if content type is supported
                bool isJson = contentType.MediaType.Contains("application/json") == true;
                bool isOctetStream = contentType.MediaType.Contains("application/octet-stream") == true;

                if (isJson == false && isOctetStream == false)
                {
                    return request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType,
                        string.Format("Request's content type ({0}) is not supported.", string.Join(", ", contentType.MediaType)));
                }

                // Check if request body is empty
                if (request.Content.Headers.ContentLength == 0)
                {
                    return request.CreateResponse(HttpStatusCode.BadRequest, "No content found in the request.");
                }

                DialogAnalysisResult result;

                if (isJson == true)
                {
                    // Read content from request
                    RequestBody requestBody = await request.Content.ReadAsAsync<RequestBody>();

                    // Verify content contains a valid image uri
                    if (requestBody.ImageUri == null || requestBody.ImageUri.IsAbsoluteUri == false)
                    {
                        return request.CreateResponse(HttpStatusCode.BadRequest, "Image uri is not initialized or valid in the request content.");
                    }

                    result = await Client.AnalyzeDialogAsync(requestBody.ImageUri);
                }
                else
                {
                    byte[] imageData;

                    // Convert stream into byte data
                    using (Stream contentStream = await request.Content.ReadAsStreamAsync())
                    {
                        // Set stream position back to 0
                        contentStream.Position = 0;

                        // Using memory stream, create byte array
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await contentStream.CopyToAsync(memoryStream);
                            imageData = memoryStream.ToArray();
                        }
                    }

                    if (imageData == null)
                    {
                        return request.CreateResponse(HttpStatusCode.BadRequest, "No binary file is found in the request content.");
                    }

                    result = await Client.AnalyzeDialogAsync(imageData);
                }

                // Return request response
                return request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                log.Error("Exception hit when analyzing dialog.", ex);
            }

            return request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to process request.");
        }

        private static string ComputerVisionApiRegion => ConfigurationManager.AppSettings["COMPUTERVISION_APP_REGION"]?.ToString();

        private static string ComputerVisionSubscriptionKey => ConfigurationManager.AppSettings["COMPUTERVISION_SUB_KEY"]?.ToString();

        private static string TextAnalyticsApiRegion => ConfigurationManager.AppSettings["TEXTANALYTICS_APP_REGION"]?.ToString();

        private static string TextAnalyticsSubscriptionKey => ConfigurationManager.AppSettings["TEXTANALYTICS_SUB_KEY"]?.ToString();
    }
}
