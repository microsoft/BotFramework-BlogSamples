using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using DialogAnalyzerFunc.Extensions;

namespace DialogAnalyzerFunc.Models
{
    [DataContract]
    public struct HandwritingRecognitionOperationResult
    {
        public enum HandwritingRecognitionOperationStatus
        {
            NotStarted = 0,
            Running = 1,
            Succeeded = 2,
            Failed = 3
        }

        [DataMember(Name = "status")]
        public HandwritingRecognitionOperationStatus Status { get; set; }

        [DataMember(Name = "recognitionResult")]
        public HandwritingRecognitionResult Result { get; set; }
    }

    [DataContract]
    public struct HandwritingRecognitionResult
    {
        [DataMember(Name = "lines")]
        public HandwritingRecognitionText[] Lines { get; set; }
    }

    [DataContract]
    public struct HandwritingRecognitionText
    {
        [DataMember(Name = "boundingBox")]
        public int[] BoundingBox { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        public ImageTextRegion TextRegion
        {
            get
            {
                // Create text region
                ImageTextRegion textRegion = new ImageTextRegion()
                {
                    Text = this.Text
                };

                // Determine boundaries
                if (this.BoundingBox.Count() == 8)
                {
                    IEnumerable<Tuple<int, int>> points = this.BoundingBox.ToTuples<int>();
                    IEnumerable<int> xAxis = points.Select(p => p.Item1).OrderBy(x => x);
                    IEnumerable<int> yAxis = points.Select(p => p.Item2).OrderBy(y => y);

                    textRegion.X = xAxis.First();
                    textRegion.Y = yAxis.First();
                    textRegion.Width = xAxis.Last() - textRegion.X;
                    textRegion.Height = yAxis.Last() - textRegion.Y;
                }

                return textRegion;
            }
        }
    }
}
