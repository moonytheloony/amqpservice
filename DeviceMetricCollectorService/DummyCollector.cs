namespace DeviceMetricCollectorService
{
    using System;
    using System.Fabric;
    using System.Threading.Tasks;

    using Amqp;
    using Amqp.Framing;
    using Amqp.Listener;

    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    public class DummyCollector : IRequestProcessor
    {
        private readonly StatefulServiceContext context;

        private readonly IReliableStateManager stateManager;

        int offset;

        public DummyCollector(StatefulServiceContext context, IReliableStateManager stateManager)
        {
            this.context = context;
            this.stateManager = stateManager;
        }

        public async void Process(RequestContext requestContext)
        {
            var myDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>("HelloCounter");
            using (var tx = this.stateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, requestContext.Link.Name);
                ServiceEventSource.Current.ServiceMessage(this.context,"Device {0} Value: {1}",requestContext.Link.Name,result.HasValue ? result.Value.ToString() : "No Hellos.");
                await myDictionary.AddOrUpdateAsync(tx, requestContext.Link.Name, 0, (key, value) => ++value);
                await tx.CommitAsync();
            }

            var task = this.ReplyAsync(requestContext);
        }

        private async Task ReplyAsync(RequestContext requestContext)
        {
            if (this.offset == 0)
            {
                this.offset = (int)requestContext.Message.ApplicationProperties["offset"];
            }

            while (this.offset < 100)
            {
                try
                {
                    var response = new Message("reply" + this.offset)
                    {
                        ApplicationProperties = new ApplicationProperties { ["offset"] = this.offset }
                    };
                    requestContext.ResponseLink.SendMessage(response);
                    this.offset++;
                }
                catch (Exception exception)
                {
                    ServiceEventSource.Current.ServiceMessage(this.context, exception.ToString());
                    if (requestContext.State == ContextState.Aborted)
                    {
                        ServiceEventSource.Current.ServiceMessage(this.context, "Request is aborted. Last offset: " + this.offset);
                        return;
                    }
                }

                await Task.Delay(1000);
            }

            requestContext.Complete(new Message("I Am Done"));
        }
    }
}