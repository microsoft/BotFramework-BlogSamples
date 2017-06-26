// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using static TriviaBot.Runtime.Utility;

namespace Luis
{
    /// <summary>
    /// Contains the default logic for converting text, or the result from the speech recognizer,
    /// into entities that will be finalized in the conversation
    /// </summary>
    public static class QueryLuis
    {
        /// <summary>
        /// Extract the best matching intent and all entities from utterance by calling LUIS with the specified LUISInfo connection data.
        /// </summary>
        /// <param name="luisInfo">Connection data for the LUIS recognizer.</param>
        /// <param name="utterance">Text to check for intents (typed or from the speech recognizer).</param>
        /// <returns>LuisResult with the intent and entities recognized by LUIS.  Entities won't be returned if intent didn't match.</returns>
        public static async Task<LuisResult> GetIntentAndEntitiesFromLuis(
            string appid, 
            string key,
            string utterance)
        {
            ArgumentNotNull(appid, "appid");
            ArgumentNotNull(key, "key");
            ArgumentNotNull(utterance, "utterance");

            // Query LUIS and generate our response
            var luisResult = await QueryLuisWithRetry(appid, key, utterance, 3);

            // LUIS can return more than one of the same entity
            // with different values and scores, we sort by the score
            // but don't retain the score otherwise.
            return luisResult;
        }

        /// <summary>
        /// Attempts to query a LUIS application, ignoring exceptions up until the last attempt
        /// </summary>
        /// <param name="luisApplication">All information needed to query the LUIS application</param>
        /// <param name="query">The text query to send to LUIS</param>
        /// <param name="retries">How many times to retry on failure</param>
        /// <returns>The deserialized data returned from LUIS</returns>
        private static async Task<LuisResult> QueryLuisWithRetry(
            string appid,
            string key,
            string query,
            int retries)
        {
            LuisResult result = null;
            Exception exception = null;
            int tooManyRequestsDelay = 125;

            for (var attempt = 0; attempt < retries; ++attempt)
            {
                exception = null;

                try
                {
                    result = await CallLuis(appid, key, query);
                    break;
                }
                catch (Exception e)
                {
                    // Check for 429 "Too many requests" error.
                    exception = e;
                    if (e.IfIs<System.Net.WebException, bool>(
                            we => we.Response.IfIs<System.Net.HttpWebResponse, bool>(
                                wr => (int)wr.StatusCode == 429, false), false))
                    {
                        await Task.Delay(tooManyRequestsDelay);
                        tooManyRequestsDelay *= 3;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Queries the specified LUIS application with the provided query
        /// </summary>
        /// <param name="luisInfo">The information needed to query the LUIS application</param>
        /// <param name="query">The query to supply the LUIS application</param>
        /// <returns>The deserialized output of the LUIS application</returns>
        private static async Task<LuisResult> CallLuis(
            string appid,
            string key,
            string query)
        {
            Stream responseStream = null;

            var requestUrl = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + appid + "?subscription-key=" + key + "&timezoneOffset=0&verbose=true&q=" + query;
            requestUrl = Uri.EscapeUriString(requestUrl);

            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            var response = await request.GetResponseAsync() as HttpWebResponse;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Error communicating with LUIS application {appid}:\nServer error (HTTP {response.StatusCode}: {response.StatusDescription}).");
            }

            responseStream = response.GetResponseStream();

            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(LuisResult));
            var objResponse = jsonSerializer.ReadObject(responseStream);

            var jsonResponse = objResponse as LuisResult;

            return jsonResponse;
        }
    }
}
