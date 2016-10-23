using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDevice
{
    using System.Threading;

    using Microsoft.ServiceFabric.Services.Communication.Client;

    public class MyCommunicationClientFactory : CommunicationClientFactoryBase<MyCommunicationClient>
    {
        protected override void AbortClient(MyCommunicationClient client)
        {
        }

        protected override Task<MyCommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
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
    }
}
