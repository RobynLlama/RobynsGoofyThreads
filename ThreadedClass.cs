using System;
using System.Threading;

public class ThreadedClass : IDisposable
{
  public readonly Action<ThreadedClass> Work;
  public readonly Action<ThreadResult>? WorkCompletedCallback;
  public readonly CancellationToken ThreadToken;
  public ThreadResult Result = ThreadResult.New;
  public Thread? WorkThread { get; private set; }
  public Exception? WorkException { get; private set; }
  private readonly CancellationTokenSource ThreadTokenSource = new();

  public ThreadedClass(Action<ThreadedClass> work, Action<ThreadResult>? workCompletedCallback = null)
  {
    Work = work;
    ThreadToken = ThreadTokenSource.Token;
    WorkCompletedCallback = workCompletedCallback;
  }

  public void RunWork()
  {
    if (Result != ThreadResult.New)
      return;

    WorkThread = new(new ThreadStart(() =>
    {
      try
      {
        Result = ThreadResult.Running;
        Work(this);

        //Check if we were cancelled or something else happened
        if (Result == ThreadResult.Running)
          Result = ThreadResult.Success;
      }
      catch (Exception ex)
      {
        Result = ThreadResult.Error;
        WorkException = ex;
      }
      finally
      {
        WorkCompletedCallback?.Invoke(Result);
      }
    }
    ));
    WorkThread.Start();
  }

  public void CancelWork()
  {
    Console.WriteLine($"Cancel requested state: {Result}");
    //if (ThreadToken.CanBeCanceled)
    switch (Result)
    {
      case ThreadResult.New:
      case ThreadResult.Running:
        Console.WriteLine("Cancelling");
        Result = ThreadResult.Cancelled;
        ThreadTokenSource.Cancel();
        break;
    }
  }

  public void Dispose()
  {
    CancelWork();
    GC.SuppressFinalize(this);
  }
}
