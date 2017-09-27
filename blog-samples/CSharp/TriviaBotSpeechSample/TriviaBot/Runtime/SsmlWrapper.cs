// Copyright (c) Microsoft Corporation. All rights reserved.

namespace TriviaBot.Runtime
{
    /// <summary>
    /// Clients the support speaking responses will expect SSML. This allows for some more complex functionality
    /// (like embedded audio, changing speaking speed, etc) but most of the time we just want to send text. This
    /// wraps text in a basic SSML template.
    /// </summary>
    public static class SsmlWrapper
    {
        /// <summary>
        /// SSML template to be filled by the properties
        /// </summary>
        private const string SSMLTemplate = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' " +
                                            "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                                            "xsi:schemaLocation='http://www.w3.org/2001/10/synthesis " +
                                            "http://www.w3.org/TR/speech-synthesis/synthesis.xsd' " +
                                            "xml:lang='{0}'><voice gender='{1}'>{2}</voice></speak>";

        /// <summary>
        /// Match pattern for SSML unwrap
        /// </summary>
        private const string RegexMatchSSMLPattern = @"^<speak.*xml:lang='(.*)'.*><voice.*gender='(.*)'.*?>([\s\S]*)<\/voice><\/speak>$";

        /// <summary>
        /// Types of gender in the voice to be used in TTS
        /// </summary>
        public enum Gender
        {
            /// <summary>
            /// Representation of a female voice
            /// </summary>
            Female,

            /// <summary>
            /// Representation of a male voice
            /// </summary>
            Male
        }

        /// <summary>
        /// Check if the input text is in SSML format
        /// </summary>
        /// <param name="text">The text to be checked</param>
        /// <returns>Return true if the input text is in SSML format, otherwise return false</returns>
        public static bool IsInSSMLFormat(string text)
        {
            return text != null && text.Contains("</speak>");
        }

        /// <summary>
        /// Generates SSML from text
        /// </summary>
        /// <param name="text">The input text to generate TTS</param>
        /// <param name="locale">Language used in the speech</param>
        /// <param name="voiceGender">Gender of the voice used in speech</param>
        /// <returns>The TTS string to be sent down to the client and played</returns>
        public static string Wrap(string text, string locale = "en-US", Gender voiceGender = Gender.Female)
        {
            if (IsInSSMLFormat(text))
            {
                return text;
            }
            else
            {
                var voiceType = voiceGender == Gender.Female ? "female" : "male";

                return string.Format(System.Globalization.CultureInfo.InvariantCulture, SSMLTemplate, locale, voiceType, text);
            }
        }
    }
}
