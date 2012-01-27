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

    var root = new DirectoryInfo(@"%PROJECTS%\Kepler\sandbox\".Expand());
    var files = root.GetFiles("*.scala", SearchOption.AllDirectories).Select(file => file.FullName.Substring(root.FullName.Length)).ToList();

    test = test.Replace("/", "\\");
    var filtered = files.Where(file => file.Contains(test)).ToList();
    if (filtered.Count == 0) {
      Console.WriteLine("warning: match not found");
      return 0;
    } else if (filtered.Count == 1) {
      Console.WriteLine("unstaging " + filtered[0]);
      test = root + filtered[0];
      var temp = Path.GetTempFileName();
      File.Copy(test, temp, true);
      Console.WriteLine(String.Format("backup: {0} now holds a copy of {1}", temp, test));
      File.Delete(test);
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
