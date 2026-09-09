using Nodal.FraudLab.Web.Domain;

namespace Nodal.FraudLab.Web.Infrastructure;

public interface IFraudCaseRepository
{
    Task<IReadOnlyList<FraudCase>> ListAsync(CancellationToken cancellationToken = default);
}
