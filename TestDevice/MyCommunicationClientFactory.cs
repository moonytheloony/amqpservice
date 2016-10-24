namespace TestDevice
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;

    public class MyCommunicationClientFactory : CommunicationClientFactoryBase<MyCommunicationClient>
    {
        public MyCommunicationClientFactory(
            IServicePartitionResolver resolver = null,
            IEnumerable<IExceptionHandler> additionalHandlers = null)
            : base(resolver, ExceptionHandlerChain(additionalHandlers))
        {
        }

        protected override void AbortClient(MyCommunicationClient client)
        {
        }

        protected override Task<MyCommunicationClient> CreateClientAsync(
            string endpoint,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new MyCommunicationClient(endpoint));
        }

        protected override bool ValidateClient(MyCommunicationClient clientChannel)
        {
            return false;
        }

        protected override bool ValidateClient(string endpoint, MyCommunicationClient client)
        {
            return false;
        }

        private static IEnumerable<IExceptionHandler> ExceptionHandlerChain(
            IEnumerable<IExceptionHandler> additionalHandlers)
        {
            return new[] { new MyExceptionHandler() }.Union(additionalHandlers ?? Enumerable.Empty<IExceptionHandler>());
        }
    }
}