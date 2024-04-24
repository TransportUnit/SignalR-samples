using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CsharpClientWpfFramework
{
    public interface ISignalRClientService
    {
        Task<IEnumerable<Stock>> GetAllStocks(CancellationToken cancellationToken = default);
        Task OpenMarket(CancellationToken cancellationToken = default);
        Task CloseMarket(CancellationToken cancellationToken = default);
        Task Reset(CancellationToken cancellationToken = default);
    }
}
