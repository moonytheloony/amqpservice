namespace DeviceMetricCollectorService
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal sealed class DeviceMetricCollectorService : StatefulService
    {
        public DeviceMetricCollectorService(StatefulServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context => new AMQPListener(context, this.StateManager), "AMQPEndpoint") };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.WaitHandle.WaitOne();
        }
    }
}