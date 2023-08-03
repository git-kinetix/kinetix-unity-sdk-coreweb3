using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CompositeTokenSource : IDisposable
{
    public readonly CancellationTokenSource compositeTokenSource;
    public          CancellationToken       Token => compositeTokenSource.Token;

    private readonly List<CancellationTokenSource> tokensSource;

    public CompositeTokenSource()
    {
        tokensSource         = new List<CancellationTokenSource>();
        compositeTokenSource = new CancellationTokenSource();
    }

    public void AddTokenSource(CancellationTokenSource tokenSource)
    {
        tokensSource.Add(tokenSource);
        tokenSource.Token.Register(CheckCancellation);
    }

    public void Dispose()
    {
       tokensSource.Clear();
    }

    private void CheckCancellation()
    {
        if (tokensSource.Any(tokenSource => !tokenSource.Token.IsCancellationRequested))
            return;

        compositeTokenSource?.Cancel();
    }
}
