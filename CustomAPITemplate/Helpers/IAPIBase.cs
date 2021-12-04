using CustomAPITemplate.Contract.V1;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Helpers;

public interface IAPIBase<TRequest> where TRequest : IRequestBase
{
    public Task<IActionResult> Get(CancellationToken token);
    public Task<IActionResult> Get(Guid id, CancellationToken token);
    public Task<IActionResult> Post(TRequest entity, CancellationToken token);
    public Task<IActionResult> Put(Guid id, TRequest entity, CancellationToken token);
    public Task<IActionResult> Delete(Guid id, CancellationToken token);
}