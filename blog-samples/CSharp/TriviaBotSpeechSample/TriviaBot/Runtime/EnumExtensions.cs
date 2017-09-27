// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TriviaBot.Runtime
{
    public static class EnumExtensions
    {
        public static string DisplayName(this Enum value)
        {
            var members = value?.GetType()?.GetMember(value.ToString());
            if (members != null)
            {
                foreach (var member in members)
                {
                    var displayName = member.GetCustomAttribute<DisplayAttribute>()?.GetName();

                    if (displayName?.Length > 0)
                    {
                        return displayName;
                    }
                }
            }

            return value?.ToString();
        }
    }
}