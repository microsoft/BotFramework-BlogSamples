// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace MIcrosoft.Bot.Sample.AzureSql.SqlStateService
{
    /// <summary>
    /// The azure bot service.
    /// </summary>
    public static class BotService
    {
        /// <summary>
        /// The bot authenticator.
        /// </summary>
        public static BotAuthenticator Authenticator => authenticator.Value;

        /// <summary>
        /// Initialize bot service by updating the <see cref="Conversation.Container"/> with <see cref="AzureModule"/>.
        /// </summary>
        /// <param name="assembly"> The assembly that should be resolved.</param>
        /// <returns> The <see cref="BotServiceScope"/> that should be disposed when bot service operation is done for the request.</returns>
        public static BotServiceScope Initialize(Assembly assembly = null)
        {
            var resolveAssembly = assembly ?? Assembly.GetCallingAssembly();

            // update the container with azure module components
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AzureModule(resolveAssembly));
            builder.Update(Conversation.Container);

            return new BotServiceScope(ResolveAssembly.Create(resolveAssembly));
        }

        /// <summary>
        /// Authenticate the request and add the service url in the activities to <see cref="MicrosoftAppCredentials.TrustedHostNames"/>.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="activities">The incoming activities.</param>
        /// <param name="token">The cancellation token</param>
        public static async Task AuthenticateAsync(HttpRequestMessage request, IEnumerable<Activity> activities, CancellationToken token = default(CancellationToken))
        {
            if (!await Authenticator.TryAuthenticateAsync(request, activities, token))
            {
                throw new UnauthorizedAccessException("Bot authentication failed!");
            }
        }

        internal static readonly Lazy<string> stateApi = new Lazy<string>(() => Utils.GetStateApiUrl());

        private static readonly Lazy<BotAuthenticator> authenticator = new Lazy<BotAuthenticator>(() => new BotAuthenticator(new StaticCredentialProvider(Utils.GetAppSetting(AppSettingKeys.AppId), Utils.GetAppSetting(AppSettingKeys.Password)),
            Utils.GetOpenIdConfigurationUrl(), false));
    }

    /// <summary>
    /// The scope for the <see cref="BotService"/>
    /// </summary>
    public sealed class BotServiceScope : IDisposable
    {
        private readonly IEnumerable<IDisposable> disposables;

        /// <summary>
        /// Creates an instance of BotServiceScope
        /// </summary>
        /// <param name="disposables"> The list of items that should be disposed when scope is disposed.</param>
        public BotServiceScope(params IDisposable[] disposables)
        {
            this.disposables = disposables;
        }

        void IDisposable.Dispose()
        {
            foreach (var disposable in this.disposables)
            {
                disposable?.Dispose();
            }
        }
    }
}