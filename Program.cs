using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Goofy
{
  internal class Program
  {
    static readonly Random rng = new();
    static void Main(string[] args)
    {

      List<ThreadedClass> threads = [];
      int running = 0;

      for (int i = 0; i < 6; i++)
      {
        Console.WriteLine("Spawning a thread");
        var work = new ThreadedClass(WorkFunction, WorkCompleted);
        work.RunWork();
        threads.Add(work);
        running++;
      }

      Console.WriteLine($"{running} threads are now running");
      Console.WriteLine("Cancelling the first task, cuz I hate it");
      threads[0].CancelWork();

      Console.WriteLine("Waiting on threads");

      foreach (var task in threads)
        task.WorkThread?.Join();

      Console.WriteLine("Thread results:");
      foreach (var task in threads)
        switch (task.Result)
        {
          case ThreadResult.Error:
            Console.WriteLine($"  ->{task.Result} ({task.WorkException?.GetType().Name}) {task.WorkException?.Message}");
            break;
          default:
            Console.WriteLine($"  ->{task.Result}");
            break;
        }
    }

    static void WorkFunction(ThreadedClass owner)
    {
      float myNumb = rng.NextSingle();
      while (myNumb < 0.7)
      {
        if (owner.ThreadToken.IsCancellationRequested)
          break;

        Console.WriteLine("A thread is doing work");

        //Randomly throw an exception or cancel the token
        //just to see it in action

        if (myNumb < 0.1)
          throw new Exception("Work got unlucky, boss");
        else if (myNumb < 0.2)
          owner.CancelWork();

        Thread.Sleep(75);
        myNumb = rng.NextSingle();
      }

      Console.WriteLine("Work done!");
    }

    static void WorkCompleted(ThreadResult result)
    {
      Console.WriteLine("Thread has ended");
    }
  }
}
