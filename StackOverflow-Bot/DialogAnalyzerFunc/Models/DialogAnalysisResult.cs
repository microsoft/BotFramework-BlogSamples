using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DialogAnalyzerFunc.Models
{
    [DataContract]
    public class DialogAnalysisResult
    {
        [DataMember(Name = "KeyPhrases")]
        public string[] KeyPhrases { get; set; } = new string[0];

        [DataMember(Name = "Labels")]
        public DialogLabel[] Labels { get; set; } = new DialogLabel[0];

        [DataMember(Name = "Tags")]
        public string[] Tags { get; set; } = new string[0];

        [DataMember(Name = "Captions")]
        public string[] Captions { get; set; } = new string[0];

        public IEnumerable<DialogLabel> ContentLabels
        {
            get
            {
                return this.Labels?.Where(label => label.DialogLabelType == DialogLabel.DialogLabelTypes.Content);
            }
        }

        public DialogLabel TitleLabel
        {
            get
            {
                return this.Labels?.FirstOrDefault(label => label.DialogLabelType == DialogLabel.DialogLabelTypes.Title);
            }
        }
    }

    [DataContract]
    public class DialogLabel
    {
        public enum DialogLabelTypes
        {
            Button,
            Content,
            Title,
            Unknown
        }

        [DataMember(Name = "DialogLabelType")]
        public DialogLabelTypes DialogLabelType { get; set; } = DialogLabelTypes.Unknown;

        [DataMember(Name = "Id")]
        public Guid Id { get; set; } = Guid.Empty;

        [DataMember(Name = "TextLabel")]
        public ImageTextRegion TextLabel { get; set; }

        public bool IsDefined => this.DialogLabelType != DialogLabelTypes.Unknown;
    }
}
