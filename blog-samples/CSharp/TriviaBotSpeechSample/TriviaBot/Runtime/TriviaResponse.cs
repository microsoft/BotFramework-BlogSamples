// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using System.Web;

namespace TriviaBot.Runtime
{
    public class TriviaQuestion
    {
        [JsonProperty("speakText")]
        public string Category;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("difficulty")]
        public string Difficulty;

        [JsonProperty("question")]
        public string Question;

        [JsonProperty("correct_answer")]
        public string CorrectAnswer;

        [JsonProperty("incorrect_answers")]
        public string[] IncorrectAnswers;

        public void Decode()
        {
            this.Category = HttpUtility.HtmlDecode(this.Category);
            this.CorrectAnswer = HttpUtility.HtmlDecode(this.CorrectAnswer);
            this.Difficulty = HttpUtility.HtmlDecode(this.Difficulty);
            for(int i = 0; i < IncorrectAnswers.Length; i++)
            {
                this.IncorrectAnswers[i] = HttpUtility.HtmlDecode(this.IncorrectAnswers[i]);
            }
            this.Question = HttpUtility.HtmlDecode(this.Question);
            this.Type = HttpUtility.HtmlDecode(this.Type);
        }
    }

    public class TriviaResponse
    {
        public void Decode()
        {
            foreach(var q in Questions)
            {
                q.Decode();
            }
        }

        [JsonProperty("response_code")]
        public int ResponseCode;


        [JsonProperty("results")]
        public TriviaQuestion[] Questions;

    }
}