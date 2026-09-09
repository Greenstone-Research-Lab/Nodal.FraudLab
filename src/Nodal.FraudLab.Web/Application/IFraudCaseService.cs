using Nodal.FraudLab.Web.Contracts;

namespace Nodal.FraudLab.Web.Application;

public interface IFraudCaseService
{
    Task<IReadOnlyList<FraudCaseSummary>> ListAsync(CancellationToken cancellationToken = default);
}
