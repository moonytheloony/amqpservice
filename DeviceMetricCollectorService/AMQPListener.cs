namespace DeviceMetricCollectorService
{
    using System;
    using System.Fabric;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using Amqp.Listener;

    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class AMQPListener : ICommunicationListener
    {
        private readonly StatefulServiceContext context;

        private readonly IReliableStateManager stateManager;

        private DeviceMetricCollectorService deviceMetricCollectorService;

        ContainerHost host;

        private string listeningAddress;

        private string publishAddress;

        public AMQPListener(StatefulServiceContext context, IReliableStateManager stateManager)
        {
            this.context = context;
            this.stateManager = stateManager;
        }

        public void Abort()
        {
            this.host.Close();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.host.Close();
            return null;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = this.context.CodePackageActivationContext.GetEndpoint("AMQPEndpoint");
            var port = serviceEndpoint.Port;
            this.listeningAddress = string.Format(CultureInfo.InvariantCulture, "amqp://guest:guest@+:{0}", port);
            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            var addressUri = new Uri(this.publishAddress);
            this.host = new ContainerHost(new[] { addressUri }, null, addressUri.UserInfo);
            this.host.Open();
            var requestProcessor = "request_processor";
            this.host.RegisterRequestProcessor(requestProcessor, new DummyCollector(this.context, this.stateManager));
            return Task.FromResult(this.publishAddress);
        }
    }
}