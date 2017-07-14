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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using Module = Autofac.Module;

namespace MIcrosoft.Bot.Sample.AzureSql.SqlStateService
{
    /// <summary>
    /// Autofac module for azure bot components.
    /// </summary>
    public sealed class AzureModule : Module
    {

        /// <summary>
        /// The key for data store register with the container.
        /// </summary>
        public static readonly object Key_DataStore = new object();

        private readonly Assembly assembly;

        /// <summary>
        /// Instantiates the azure module. 
        /// </summary>
        /// <param name="assembly">
        /// The assembly used by <see cref="BotServiceDelegateSurrogate"/> and
        /// <see cref="BotServiceSerializationBinder"/>
        /// </param>
        public AzureModule(Assembly assembly)
        {
            SetField.NotNull(out this.assembly, nameof(assembly), assembly);
        }

        /// <summary>
        /// Registers dependencies with the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"> The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectorStore>()
                .AsSelf()
                .InstancePerLifetimeScope();

            // if application settings indicate that bot should use the table storage, 
            // TableBotDataStore will be registered as underlying storage
            // otherwise bot connector state service will be used.
            if (ShouldUseTableStorage())
            {
                builder.Register(c => MakeTableBotDataStore())
                    .Keyed<IBotDataStore<BotData>>(Key_DataStore)
                    .AsSelf()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new ConnectorStore(c.Resolve<IStateClient>()))
                    .Keyed<IBotDataStore<BotData>>(Key_DataStore)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }

            // register the data store with caching data store
            // and set the consistency policy to be "Last write wins".
            builder.Register(c => new CachingBotDataStore(c.ResolveKeyed<IBotDataStore<BotData>>(Key_DataStore),
                        CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();

            // register the appropriate StateClient based on the state api url.
            builder.Register(c =>
            {
                var activity = c.Resolve<IActivity>();
                if (activity.ChannelId == "emulator")
                {
                    // for emulator we should use serviceUri of the emulator for storage
                    return new StateClient(new Uri(activity.ServiceUrl));
                }

                MicrosoftAppCredentials.TrustServiceUrl(BotService.stateApi.Value, DateTime.MaxValue);
                return new StateClient(new Uri(BotService.stateApi.Value));
            })
            .As<IStateClient>()
            .InstancePerLifetimeScope();

            // register the bot service serialization binder for type mapping to current assembly
            builder.Register(c => new BotServiceSerializationBinder(assembly))
                .AsSelf()
                .As<SerializationBinder>()
                .InstancePerLifetimeScope();

            // register the Delegate surrogate provide to map delegate to current assembly during deserialization
            builder
                .Register(c => new BotServiceDelegateSurrogate(assembly))
                .AsSelf()
                .InstancePerLifetimeScope();

            // extend surrogate providers with bot service delegate surrogate provider and register the surrogate selector
            builder
                .Register(c =>
                {
                    var providers = c.ResolveKeyed<IEnumerable<Serialization.ISurrogateProvider>>(FiberModule.Key_SurrogateProvider).ToList();
                    // need to add the latest delegate surrogate to make sure that surrogate selector
                    // can deal with latest assembly
                    providers.Add(c.Resolve<BotServiceDelegateSurrogate>());
                    return new Serialization.SurrogateSelector(providers);
                })
                .As<ISurrogateSelector>()
                .InstancePerLifetimeScope();

            // register binary formatter used for binary serialization operation
            builder
                .Register((c, p) => new BinaryFormatter(c.Resolve<ISurrogateSelector>(), new StreamingContext(StreamingContextStates.All, c.Resolve<IResolver>(p)))
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    Binder = c.Resolve<SerializationBinder>()
                })
                .As<IFormatter>()
                .InstancePerLifetimeScope();
        }

        private bool ShouldUseTableStorage()
        {
            bool shouldUseTableStorage = false;
            var useTableStore = Utils.GetAppSetting(AppSettingKeys.UseTableStorageForConversationState);
            return bool.TryParse(useTableStore, out shouldUseTableStorage) && shouldUseTableStorage;
        }

        private TableBotDataStore MakeTableBotDataStore()
        {
            var connectionString = Utils.GetAppSetting(AppSettingKeys.TableStorageConnectionString);

            if (!string.IsNullOrEmpty(connectionString))
            {
                return new TableBotDataStore(connectionString);
            }

            // no connection string in application settings but should use table storage flag is set.
            throw new ArgumentException("connection string for table storage is not set in application setting.");
        }
    }
}
