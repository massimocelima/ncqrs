using System;
using Castle.DynamicProxy;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing.Snapshotting.DynamicSnapshot
{
    internal class SnapshotableAggregateRootFactory
    {
        private readonly SnapshotableImplementerFactory _snapshotableImplementerFactory;
        private readonly IDynamicSnapshotAssembly _dynamicSnapshotAssembly;

        private readonly static ProxyGenerator Generator = new ProxyGenerator();

        public SnapshotableAggregateRootFactory(IDynamicSnapshotAssembly dynamicSnapshotAssembly, SnapshotableImplementerFactory snapshotableImplementerFactory)
        {
            _snapshotableImplementerFactory = snapshotableImplementerFactory;
            _dynamicSnapshotAssembly = dynamicSnapshotAssembly;
        }

        public AggregateRoot Create(Type aggregateType)
        {
            if (!typeof(AggregateRoot).IsAssignableFrom(aggregateType))
                throw new ArgumentException("aggregateType must inherit AggregateRoot");

            var snapshotType = _dynamicSnapshotAssembly.FindSnapshotType(aggregateType);
            var snapshotableImplementer = _snapshotableImplementerFactory.Create(snapshotType);

            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(snapshotableImplementer);

            var proxy = (AggregateRoot)Generator.CreateClassProxy(aggregateType, options);
            ((IHaveProxyReference)proxy).Proxy = proxy;

            return proxy;
        }

    }
}
