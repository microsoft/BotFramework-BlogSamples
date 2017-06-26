// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace TriviaBot
{
    public static class Extensions
    {
        public static string Normalize(this string msg)
        {
            return msg.ToLower().Trim().TrimEnd('.');
        }

        public static bool NormalizedEquals(this string msg, string other)
        {
            return Normalize(msg) == Normalize(other);
        }

        public static bool ContainsIgnoreCase(this string msg, IEnumerable<string> parts)
        {
            var msgL = msg?.ToLower();
            return parts.Any(x => msgL?.Contains(x.ToLower()) == true);
        }
    }
}