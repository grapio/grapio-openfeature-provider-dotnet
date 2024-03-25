using Grpc.Core;

namespace Grapio.Provider.Tests;

internal class FeatureFlagAsyncStreamReader<T>(IEnumerable<T> results) : IAsyncStreamReader<T>
{
    private readonly IEnumerator<T> _enumerator = results.GetEnumerator();

    public T Current => _enumerator.Current;

    public Task<bool> MoveNext(CancellationToken cancellationToken) =>
        Task.Run(() => _enumerator.MoveNext(), cancellationToken);
}