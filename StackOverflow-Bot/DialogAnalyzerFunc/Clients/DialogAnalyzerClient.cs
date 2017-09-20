using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DialogAnalyzerFunc.Models;
using DialogAnalyzerFunc.Services;
using DialogAnalyzerFunc.Utilities;

namespace DialogAnalyzerFunc.Clients
{
    public class DialogAnalyzerClient
    {
        private ComputerVisionService ComputerVisionService;
        private TextAnalyticsService TextAnalyticsService;

        public DialogAnalyzerClient(string computerVisionApiRegion, string computerVisionSubscriptionKey,
            string textAnalyticsApiRegion, string textAnalyticsSubscriptionKey)
        {
            // Computer Vision Service
            this.ComputerVisionService = new ComputerVisionService(computerVisionApiRegion, computerVisionSubscriptionKey);

            // Text Analytics Service
            this.TextAnalyticsService = new TextAnalyticsService(textAnalyticsApiRegion, textAnalyticsSubscriptionKey);
        }

        /// <summary>
        /// Analyze dialog with image data
        /// </summary>
        public async Task<DialogAnalysisResult> AnalyzeDialogAsync(byte[] imageData)
        {
            if (imageData?.Count() > 0 == false)
            {
                throw new ArgumentNullException("Image data is not initialized.");
            }

            // Run handwritten text recognition service
            Task<HandwritingRecognitionResult> hwrTask = this.ComputerVisionService.RecognizeHandwrittenTextAsync(imageData);

            // Run analyze image service
            Task<ComputerVisionImageAnalysisResult> imageTask = this.ComputerVisionService.AnalyzeImageAsync(imageData);

            // Wait for all tasks to be completed
            await Task.WhenAll(hwrTask, imageTask);

            // Get results
            return await InterpretResultsAsync(hwrTask.Result, imageTask.Result);
        }

        /// <summary>
        /// Analyze dialog with image uri
        /// </summary>
        public async Task<DialogAnalysisResult> AnalyzeDialogAsync(Uri imageUri)
        {
            if (imageUri == null)
            {
                throw new ArgumentNullException("Image uri is not initialized.");
            }

            // Run handwritten text recognition service
            Task<HandwritingRecognitionResult> hwrTask = this.ComputerVisionService.RecognizeHandwrittenTextAsync(imageUri);

            // Run analyze image service
            Task<ComputerVisionImageAnalysisResult> imageTask = this.ComputerVisionService.AnalyzeImageAsync(imageUri);

            // Wait for all tasks to be completed
            await Task.WhenAll(hwrTask, imageTask);

            // Get results
            return await InterpretResultsAsync(hwrTask.Result, imageTask.Result);
        }

        private async Task<DialogAnalysisResult> InterpretResultsAsync(HandwritingRecognitionResult hwrResult, ComputerVisionImageAnalysisResult imageResult)
        {
            DialogAnalysisResult retResult = new DialogAnalysisResult();

            if (hwrResult.Lines?.Count() > 0)
            {
                // Get labels from handwriting recognition
                IEnumerable<ImageTextRegion> labels = hwrResult.Lines.Where(line => string.IsNullOrEmpty(line.Text) == false).Select(line => line.TextRegion);

                // Interpret dialog data
                DialogDataInterpreter dialogDataInterpreter = new DialogDataInterpreter(imageResult.Metadata.Height, imageResult.Metadata.Width, labels);
                retResult = dialogDataInterpreter.Result;

                // Extract text from dialog data interpreter's title and content result
                List<string> results = new List<string>();
                results.Add(retResult.TitleLabel?.TextLabel?.Text);
                results.AddRange(retResult.ContentLabels?.Select(label => label?.TextLabel?.Text));

                // Analyze key phrases from result text
                string text = StringUtility.GetTextOrDefault(results, string.Empty, " ");
                if (string.IsNullOrEmpty(text) == false)
                {
                    TextAnalyticsResult<TextAnalyticsKeyPhrasesResult> keyPhrasesResult = await this.TextAnalyticsService.AnalyzeKeyPhrasesAsync(text);
                    retResult.KeyPhrases = keyPhrasesResult.Results?.Select(kp => kp.KeyPhrases).FirstOrDefault();
                }
            }

            // Add image description tags
            if (imageResult.Description?.Tags?.Count() > 0)
            {
                retResult.Tags = imageResult.Description.Tags;
            }

            // Add image description captions
            if (imageResult.Description?.Captions?.Count() > 0)
            {
                retResult.Captions =
                    imageResult.Description.Captions.Where(cap => string.IsNullOrEmpty(cap.Text) == false)
                        .OrderByDescending(cap => cap.Confidence).Select(cap => cap.Text).ToArray();
            }

            return retResult;
        }
    }
}
