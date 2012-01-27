using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  public static int Main(String[] args) {
    String test = null;
    if (args.Length == 0) {
      Console.Write("Test: ");
      test = Console.ReadLine();
    } else {
      test = String.Join(" ", args);
    }

    var dottest = new FileInfo(@"%PROJECTS%\Kepler\.test".Expand());
    if (!dottest.Exists) File.WriteAllText(dottest.FullName, "");
    var tests = File.ReadAllLines(dottest.FullName).ToList();
    var filtered = tests.Where(test1 => test1.Contains(test)).ToList();

    if (filtered.Count == 0) {
      Console.WriteLine("warning: match not found");
      return 0;
    } else if (filtered.Count == 1) {
      Console.WriteLine("untesting: " + filtered[0]);
      tests.Remove(filtered[0]);
      File.WriteAllText(dottest.FullName, String.Join(Environment.NewLine, tests.ToArray()));
      return 0;
    } else {
      Console.WriteLine("error: match is ambiguous");
      filtered.Take(5).ToList().ForEach(file => Console.WriteLine("    " + file));
      if (filtered.Count > 5) Console.WriteLine("    ... " + (filtered.Count - 5) + " more");
      return -1;
    }
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}
