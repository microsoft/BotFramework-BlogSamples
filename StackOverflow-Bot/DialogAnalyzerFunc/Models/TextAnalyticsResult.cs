using System;
using System.Runtime.Serialization;

namespace DialogAnalyzerFunc.Models
{
    [DataContract]
    public class TextAnalyticsResult<T>
    {
        [DataMember(Name = "documents")]
        public T[] Results { get; set; }
    }

    [DataContract]
    public class TextAnalyticsKeyPhrasesResult
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "keyPhrases")]
        public string[] KeyPhrases { get; set; }
    }
}
