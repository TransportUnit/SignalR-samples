using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CsharpClientWpfFramework
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ISignalRClientService _signalRClientService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Func<StockViewModel> _stockViewModelFactory;
        private readonly SemaphoreSlim _semaphore;
        private bool _loaded;

        private ObservableCollection<StockViewModel> _stocks;
        public ObservableCollection<StockViewModel> Stocks
        {
            get { return _stocks; }
            set { SetProperty(ref _stocks, value); }
        }

        private MarketState _marketState;
        public MarketState MarketState
        {
            get { return _marketState; }
            set { SetProperty(ref _marketState, value); }
        }

        private ICommand _loadedCommand;
        public ICommand LoadedCommand
            =>
            _loadedCommand ??
            (_loadedCommand = new DelegateCommand(async () =>
                {
                    if (_loaded)
                        return;

                    _loaded = true;

                    await GetAllStocks();
                },
                () => !_loaded));

        private ICommand _getAllStocksCommand;
        public ICommand GetAllStocksCommand
            =>
            _getAllStocksCommand ??
            (_getAllStocksCommand = new DelegateCommand(async () => await GetAllStocks()));

        private ICommand _openMarketCommand;
        public ICommand OpenMarketCommand
            =>
            _openMarketCommand ??
            (_openMarketCommand = new DelegateCommand(async () => await OpenMarket(), () => MarketState != MarketState.Opened).ObservesProperty(() => MarketState));

        private ICommand _closeMarketCommand;
        public ICommand CloseMarketCommand
            =>
            _closeMarketCommand ??
            (_closeMarketCommand = new DelegateCommand(async () => await CloseMarket(), () => MarketState == MarketState.Opened).ObservesProperty(() => MarketState));

        private ICommand _resetCommand;
        public ICommand ResetCommand
            =>
            _resetCommand ??
            (_resetCommand = new DelegateCommand(async () => { await Reset(); await GetAllStocks(); }, () => MarketState != MarketState.Opened).ObservesProperty(() => MarketState));


        public MainWindowViewModel(
            ISignalRClientService signalRClientService,
            IEventAggregator eventAggregator,
            Func<StockViewModel> stockViewModelFactory
            )
        {
            _signalRClientService = signalRClientService;
            _eventAggregator = eventAggregator;
            _stockViewModelFactory = stockViewModelFactory;

            _semaphore = new SemaphoreSlim(1, 1);

            Stocks = new ObservableCollection<StockViewModel>();
            MarketState = MarketState.Closed;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventAggregator
                .GetEvent<StockEvent>()
                .Subscribe(
                    async (stock) => await HandleStockEvent(stock),
                    ThreadOption.UIThread,
                    false
                );

            _eventAggregator
                .GetEvent<MarketStateEvent>()
                .Subscribe(
                    (marketState) => MarketState = marketState,
                    ThreadOption.UIThread,
                    false
                );
        }

        private async Task HandleStockEvent(
            Stock stock,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var stockViewModel = _stocks.FirstOrDefault(x => x.Symbol == stock.Symbol);

                if (stockViewModel == null) 
                {
                    stockViewModel = _stockViewModelFactory();
                    Stocks.Add(stockViewModel);
                }

                stockViewModel.Initialize(stock);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task GetAllStocks(CancellationToken cancellationToken = default)
        {
            var stocks = await _signalRClientService.GetAllStocks(cancellationToken);

            foreach (var stock in stocks)
            {
                await HandleStockEvent(stock, cancellationToken);
            }
        }

        private async Task OpenMarket(CancellationToken cancellationToken = default)
        {
            await _signalRClientService.OpenMarket(cancellationToken);
        }

        private async Task CloseMarket(CancellationToken cancellationToken = default)
        {
            await _signalRClientService.CloseMarket(cancellationToken);
        }

        private async Task Reset(CancellationToken cancellationToken = default)
        {
            await _signalRClientService.Reset(cancellationToken);
        }
    }
}
