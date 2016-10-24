namespace TestDevice
{
    using System;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;

    using Amqp;
    using Amqp.Framing;

    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;

    class Program
    {
        private static MyCommunicationClientFactory myCommunicationClientFactory;
        private static readonly Uri myServiceUri = new Uri("fabric:/MetricCollector/DeviceMetricCollectorService");

        static void Main(string[] args)
        {
            myCommunicationClientFactory = new MyCommunicationClientFactory();
            var partition = new ServicePartitionKey(1);
            var myServicePartitionClient = new ServicePartitionClient<MyCommunicationClient>(myCommunicationClientFactory, myServiceUri, partition);
            Console.WriteLine("Running request client...");
            var result =
                myServicePartitionClient.InvokeWithRetryAsync(client => client.Run(), CancellationToken.None).Result;
            Console.ReadKey();
        }
    }
}