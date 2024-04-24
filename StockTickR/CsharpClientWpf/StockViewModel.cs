using Prism.Mvvm;

namespace CsharpClientWpfFramework
{
    public class StockViewModel : BindableBase
    {
        private bool _initialized;

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set { SetProperty(ref _symbol, value); }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set 
            {
                if (value != _price && _initialized)
                    SetChangeFlag(value > _price);

                SetProperty(ref _price, value);
            }
        }

        private decimal _dayOpen;
        public decimal DayOpen
        {
            get { return _dayOpen; }
            set { SetProperty(ref _dayOpen, value); }
        }

        private decimal _dayLow;
        public decimal DayLow
        {
            get { return _dayLow; }
            set { SetProperty(ref _dayLow, value); }
        }

        private decimal _dayHigh;
        public decimal DayHigh
        {
            get { return _dayHigh; }
            set { SetProperty(ref _dayHigh, value); }
        }

        private decimal _lastChange;
        public decimal LastChange
        {
            get { return _lastChange; }
            set { SetProperty(ref _lastChange, value); }
        }

        private decimal _change;
        public decimal Change
        {
            get { return _change; }
            set 
            { 
                if (SetProperty(ref _change, value))
                {
                    RaisePropertyChanged(nameof(ChangePositive));
                    RaisePropertyChanged(nameof(ChangeNeutral));
                }
            }
        }

        private double _percentChange;
        public double PercentChange
            {
            get { return _percentChange; }
            set { SetProperty(ref _percentChange, value); }
        }

        private int _changeFlag;
        public int ChangeFlag
        {
            get { return _changeFlag; }
            set { SetProperty(ref _changeFlag, value); }
        }

        private void SetChangeFlag(bool up)
        {
            // this is stupid
            if (up)
            {
                if (ChangeFlag == 1)
                    ChangeFlag = 2;
                else
                    ChangeFlag = 1;
            }
            else
            {
                if (ChangeFlag == -1)
                    ChangeFlag = -2;
                else
                    ChangeFlag = -1;
            }
        }

        public bool ChangePositive => Change >= 0;
        public bool ChangeNeutral => Change == 0;

        public void Initialize(Stock stock)
        {
            this.Symbol = stock.Symbol;
            this.Price = stock.Price;
            this.DayOpen = stock.DayOpen;
            this.DayLow = stock.DayLow;
            this.DayHigh = stock.DayHigh;
            this.LastChange = stock.LastChange;
            this.Change = stock.Change;
            this.PercentChange = stock.PercentChange;
            _initialized = true;
        }
    }
}
