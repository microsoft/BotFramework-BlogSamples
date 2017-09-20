using System;
using System.Runtime.Serialization;

namespace DialogAnalyzerFunc.Models
{
    [DataContract]
    public class ComputerVisionImageAnalysisResult
    {
        [DataMember(Name = "categories")]
        public ComputerVisionImageCategory[] Categories { get; set; }

        [DataMember(Name = "description")]
        public ComputerVisionImageDescription Description { get; set; }

        [DataMember(Name = "metadata")]
        public ComputerVisionImageMetadata Metadata { get; set; }

        [DataMember(Name = "requestId")]
        public string RequestId { get; set; }
    }

    [DataContract]
    public class ComputerVisionImageCategory
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "score")]
        public double Score { get; set; }
    }

    [DataContract]
    public class ComputerVisionImageCaption
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "confidence")]
        public double Confidence { get; set; }
    }

    [DataContract]
    public class ComputerVisionImageDescription
    {
        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "captions")]
        public ComputerVisionImageCaption[] Captions { get; set; }
    }

    [DataContract]
    public class ComputerVisionImageMetadata
    {
        [DataMember(Name = "format")]
        public string Format { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }
    }
}
