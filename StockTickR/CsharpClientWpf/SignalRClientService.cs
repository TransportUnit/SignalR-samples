using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CsharpClientWpfFramework
{
    public class SignalRClientService : ISignalRClientService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly HubConnection _hubConnection;
        public Task Initialization { get; private set; }

        public SignalRClientService(
            IEventAggregator eventAggregator
            )
        {
            _eventAggregator = eventAggregator;

            // url can be resolved from a config file etc. pp

            // make sure the port matches with the port in StockTickR/Properties/launchSettings.json

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/stocks")
                //.ConfigureLogging(logging =>
                //{
                //    logging.AddConsole();
                //})
                .AddMessagePackProtocol()
                .Build();

            _hubConnection.On("marketOpened", () =>
            {
                _eventAggregator
                        .GetEvent<MarketStateEvent>()
                        .Publish(MarketState.Opened);
            });

            _hubConnection.On("marketClosed", () =>
            {
                _eventAggregator
                       .GetEvent<MarketStateEvent>()
                       .Publish(MarketState.Closed);
            });

            _hubConnection.On("marketReset", () =>
            {
                _eventAggregator
                        .GetEvent<MarketStateEvent>()
                        .Publish(MarketState.Reset);
            });

            Initialization = Initialize();
        }

        private async Task Initialize(CancellationToken cancellationToken = default)
        {
            await _hubConnection.StartAsync();

            var channel = await _hubConnection.StreamAsChannelAsync<Stock>("StreamStocks", CancellationToken.None);

            // use cancellation token to control start and stop
            while (await channel.WaitToReadAsync() && !cancellationToken.IsCancellationRequested)
            {
                while (channel.TryRead(out var stock))
                {
                    _eventAggregator
                        .GetEvent<StockEvent>()
                        .Publish(stock);
                }
            }
        }

        public async Task<IEnumerable<Stock>> GetAllStocks(CancellationToken cancellationToken = default)
        {
            return await _hubConnection.InvokeAsync<IEnumerable<Stock>>("GetAllStocks", cancellationToken);
        }

        public async Task OpenMarket(CancellationToken cancellationToken = default)
        {
            await _hubConnection.InvokeAsync("OpenMarket", cancellationToken);
        }

        public async Task CloseMarket(CancellationToken cancellationToken = default)
        {
            await _hubConnection.InvokeAsync("CloseMarket", cancellationToken);
        }

        public async Task Reset(CancellationToken cancellationToken = default)
        {
            await _hubConnection.InvokeAsync("Reset", cancellationToken);
        }
    }
}
