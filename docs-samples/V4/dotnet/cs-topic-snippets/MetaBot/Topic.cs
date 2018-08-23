using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaBot
{
    /// <summary>Represents a topic on the docs site.</summary>
    public class Topic
    {
        public string Name { get; set; }
        public string File { get; set; }

        /// <summary>The bots containing snippet code for topic,
        /// indexed by the section in which they appear.</summary>
        public IDictionary<string, IBot> Sections { get; set; }
    }
}
