using System;
using System.Collections.Generic;
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
      var dottest = new FileInfo(@"%PROJECTS%\Kepler\.test".Expand());
      var tested = dottest.Exists ? File.ReadAllLines(dottest.FullName).ToList() : new List<String>();
      Console.WriteLine("{0} file{1} tested", tested.Count, tested.Count != 1 ? "s" : "");
      tested.ForEach(test1 => Console.WriteLine("    " + test1));
      return 0;
    } else {
      test = String.Join(" ", args);
    }

    var root = new DirectoryInfo(@"%PROJECTS%\Kepler\test\".Expand());
    var loc1 = new DirectoryInfo(@"%PROJECTS%\Kepler\test\files\".Expand());
    var files = loc1.GetFiles("*.scala", SearchOption.AllDirectories).Select(file => file.FullName.Substring(root.FullName.Length)).ToList();
    var loc2 = new DirectoryInfo(@"%PROJECTS%\Kepler\test\pending\".Expand());
    files = files.Concat(loc2.GetFiles("*.scala", SearchOption.AllDirectories).Select(file => file.FullName.Substring(root.FullName.Length))).ToList();

    test = test.Replace("/", "\\");
    var filtered = files.Where(file => file.Contains(test)).ToList();
    if (filtered.Count == 0) {
      Console.WriteLine("error: match not found");
      return -1;
    } else if (filtered.Count == 1) {
      test = @"test\" + filtered[0];

      Console.WriteLine("adding test: " + test);
      var dottest = new FileInfo(@"%PROJECTS%\Kepler\.test".Expand());
      if (!dottest.Exists) File.WriteAllText(dottest.FullName, "");
      var tests = File.ReadAllLines(dottest.FullName).ToList();
      if (!tests.Contains(test)) tests.Add(test);
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
