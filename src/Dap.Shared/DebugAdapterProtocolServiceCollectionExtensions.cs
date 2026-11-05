using System;
using DryIoc;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.JsonRpc;

namespace OmniSharp.Extensions.DebugAdapter.Shared
{
    internal static class DebugAdapterProtocolServiceCollectionExtensions
    {
        internal static IContainer AddDebugAdapterProtocolInternals<T>(this IContainer container, DebugAdapterRpcOptionsBase<T> options) where T : IJsonRpcHandlerRegistry<T>
        {
            if (options.Serializer == null)
            {
                throw new ArgumentException("Serializer is missing!", nameof(options));
            }

            container = container.AddJsonRpcServerCore(options);

            if (options.UseAssemblyAttributeScanning)
            {
                container.RegisterInstanceMany(new AssemblyAttributeHandlerTypeDescriptorProvider(options.Assemblies), nonPublicServiceTypes: true);
            }
            else
            {
                container.RegisterInstanceMany(new AssemblyScanningHandlerTypeDescriptorProvider(options.Assemblies), nonPublicServiceTypes: true);
            }

            container.RegisterInstanceMany(options.Serializer);
            container.RegisterInstance(options.RequestProcessIdentifier);
            container.RegisterMany<DebugAdapterSettingsBag>(nonPublicServiceTypes: true, reuse: Reuse.Singleton);
            container.RegisterMany<DapReceiver>(nonPublicServiceTypes: true, reuse: Reuse.Singleton);
            container.RegisterMany<DapOutputFilter>(nonPublicServiceTypes: true, reuse: Reuse.Singleton);
            container.RegisterMany<DebugAdapterRequestRouter>(Reuse.Singleton);
            container.RegisterDelegate(context => {
                var manager = new DebugAdapterHandlerCollection(context, context.Resolve<IHandlerTypeDescriptorProvider<IHandlerTypeDescriptor?>>());
                context.Resolve<IJsonRpcHandlerCollection>().Populate(context, manager);
                return manager;
            }, Reuse.Singleton);
            container.RegisterDelegate<IHandlersManager>(context => context.Resolve<DebugAdapterHandlerCollection>(), Reuse.Singleton);
            container.RegisterMany<DapResponseRouter>(Reuse.Singleton);

            return container;
        }
    }
}
