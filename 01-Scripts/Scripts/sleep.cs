// build this with "csc /t:exe sleep.cs"

using System;
using System.Threading;

public class App {
  public static int Main(String[] args) {
    if (args.Length != 1) {
      Console.WriteLine("usage: sleep MILLISECONDS");
      return -1;
    }

    int milliseconds;
    if (!int.TryParse(args[0], out milliseconds)) {
      Console.WriteLine("usage: sleep MILLISECONDS");
      return -1;
    }

    Thread.Sleep(milliseconds);
    return 0;
  }
}