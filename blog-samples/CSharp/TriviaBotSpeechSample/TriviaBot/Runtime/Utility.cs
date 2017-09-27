// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace TriviaBot.Runtime
{
    public static class Utility
    {
        /// <summary>
        /// Perform an action if the type of baseObject matches the type of the action
        /// </summary>
        /// <typeparam name="TDerived">The type of the action</typeparam>
        /// <param name="baseObject">The object to operate on</param>
        /// <param name="doIfDerived">The action to execute if the types match</param>
        public static void IfIs<TDerived>(this object baseObject, Action<TDerived> doIfDerived)
            where TDerived : class
        {
            var derivedObject = baseObject as TDerived;
            if (derivedObject != null)
            {
                doIfDerived(derivedObject);
            }
        }

        /// <summary>
        /// Perform an action if the type of baseObject matches the type of the action, otherwise returns elseResult
        /// </summary>
        /// <typeparam name="TDerived">The type of the action</typeparam>
        /// <typeparam name="TResult">The type of the result to return</typeparam>
        /// <param name="baseObject">The object to operate on</param>
        /// <param name="doIfDerived">The action to execute if the types match</param>
        /// <param name="elseResult">The value to return if baseObject is not of type TDerived</param>
        /// <returns>The result of running doIfDerived on baseObject if possible, otherwise elseResult</returns>
        public static TResult IfIs<TDerived, TResult>(
            this object baseObject,
            Func<TDerived, TResult> doIfDerived,
            TResult elseResult)
            where TDerived : class
        {
            var derivedObject = baseObject as TDerived;
            return derivedObject != null
                ? doIfDerived(derivedObject)
                : elseResult;
        }

        /// <summary>
        /// Verifies that an argument is not null, and throws an exception if it is.
        /// </summary>
        /// <typeparam name="TArg">The argument type to verify</typeparam>
        /// <param name="arg">The argument to verify</param>
        /// <param name="name">The name to display in the thrown exception</param>
        public static void ArgumentNotNull<TArg>(TArg arg, string name)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}