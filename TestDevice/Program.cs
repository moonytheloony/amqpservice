namespace TestDevice
{
    using System;
    using System.Threading;

    using Amqp;
    using Amqp.Framing;

    class Program
    {
        static void Main(string[] args)
        {
            var address = "amqp://guest:guest@localhost:5672";
            if (args.Length > 0)
            {
                address = args[0];
            }

            Console.WriteLine("Running request client...");
            new Client(address).Run();
            Console.ReadKey();
        }

        class Client
        {
            readonly string address;

            Connection connection;

            int offset;

            ReceiverLink receiver;

            readonly string replyTo;

            SenderLink sender;

            Session session;

            public Client(string address)
            {
                this.address = address;
                this.replyTo = "client-" + Guid.NewGuid();
            }

            public void Run()
            {
                while (true)
                {
                    try
                    {
                        this.Cleanup();
                        this.Setup();
                        this.RunOnce();
                        this.Cleanup();
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Reconnect on exception: " + exception.Message);
                        Thread.Sleep(5000);
                    }
                }
            }

            void Cleanup()
            {
                var temp = Interlocked.Exchange(ref this.connection, null);
                temp?.Close();
            }

            void RunOnce()
            {
                var request = new Message("hello " + this.offset)
                    {
                        Properties = new Properties { MessageId = "command-request", ReplyTo = this.replyTo },
                        ApplicationProperties = new ApplicationProperties { ["offset"] = this.offset }
                    };
                this.sender.Send(request, null, null);
                Console.WriteLine($"Sent request {request.Properties} body {request.Body}");

                var response = this.receiver.Receive();
                this.receiver.Accept(response);
                Console.WriteLine($"Received response: {response.Properties} body {response.Body}");
                if ("done" == (string)response.Body)
                {
                    return;
                }

                this.offset = (int)response.ApplicationProperties["offset"] + 1;
            }

            void Setup()
            {
                this.connection = new Connection(new Address(this.address));
                this.session = new Session(this.connection);
                var recvAttach = new Attach
                {
                    Source = new Source { Address = "request_processor" },
                    Target = new Target { Address = this.replyTo }
                };

                this.receiver = new ReceiverLink(this.session, "request-client-receiver", recvAttach, null);
                this.receiver.Start(300);
                this.sender = new SenderLink(this.session, "request-client-sender", "request_processor");
            }
        }
    }
}