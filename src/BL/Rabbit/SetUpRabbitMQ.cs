using Serilog;
using EasyNetQ.Management.Client;
using EasyNetQ.Management.Client.Model;

namespace BL.Rabbit
{
    public static class SetUpRabbitMQ
    {
        public static async Task DoItAsync(string hostname, string portNumber, string virtualHost, string username,
            string password)
        {
            Log.Information("{Function} Rabbit Conf: host={Host}", "DoItAsync", hostname);
            Log.Information("{Function} Rabbit Conf: port={Port}", "DoItAsync", portNumber);
            Log.Information("{Function} Rabbit Conf: vhost={VHost}", "DoItAsync", virtualHost);

            try
            {

                var initial = new ManagementClient(hostname, username, password);

                // Create dev vhost as used by so many other things
                await initial.CreateVhostAsync(virtualHost);

                var vhost = await initial.GetVhostAsync(virtualHost);
                // Create main exchange
                await initial.CreateExchangeAsync(new ExchangeInfo(ExchangeConstants.Main, "topic"), vhost);

                var exchange = await initial.GetExchangeAsync(vhost, ExchangeConstants.Main);

                // create a queue users
                await initial.CreateQueueAsync(new QueueInfo(QueueConstants.Submissions), vhost);
                var subs = await initial.GetQueueAsync(vhost, QueueConstants.Submissions);
                await initial.CreateQueueBindingAsync(exchange, subs, new BindingInfo(RoutingConstants.Subs));

                await initial.CreateQueueAsync(new QueueInfo(QueueConstants.FetchExtarnalFile), vhost);
                var fetchFile = await initial.GetQueueAsync(vhost, QueueConstants.FetchExtarnalFile);
                await initial.CreateQueueBindingAsync(exchange, fetchFile, new BindingInfo(RoutingConstants.FetchFile));



            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "DoItAsync");
            }
        }
    }
}



