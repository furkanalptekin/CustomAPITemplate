using CustomAPITemplate.Contract.V1;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Helpers;

public interface IApiBase<TKey, TRequest> where TRequest : IRequestBase
{
    public Task<IActionResult> Get(CancellationToken token);
    public Task<IActionResult> Get(TKey id, CancellationToken token);
    public Task<IActionResult> Post(TRequest entity, CancellationToken token);
    public Task<IActionResult> Put(TKey id, TRequest entity, CancellationToken token);
    public Task<IActionResult> Delete(TKey id, CancellationToken token);
}