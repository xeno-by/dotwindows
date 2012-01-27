// build this with "csc /r:Microsoft.VisualBasic.dll /t:exe /out:unstage.exe unstage.cs"
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

public class App {
  public static int Main(String[] args) {
    String test = null;
    if (args.Length == 0) {
      Console.WriteLine("usage: unstage <mask>");
      return -1;
    } else {
      test = String.Join(" ", args);
    }

    var root = new DirectoryInfo(@"%PROJECTS%\Kepler\sandbox\".Expand());
    var files = root.GetFiles("*.scala", SearchOption.AllDirectories).Select(file => file.FullName.Substring(root.FullName.Length)).ToList();

    Func<String, int> impl = wannabe => {
      Console.WriteLine("unstaging: " + wannabe);
      wannabe = root + wannabe;
      if (File.Exists(wannabe)) {
        var temp = Path.GetTempFileName();
        File.Copy(wannabe, temp, true);
        Console.WriteLine(String.Format("backup: {0} now holds a copy of {1}", temp, wannabe));
        File.Delete(wannabe);
      }
      return 0;
    };

    test = test.Replace("/", "\\");
    var filtered = files.Where(file => file.MatchesWildcard(test)).ToList();
    if (filtered.Count == 0) {
      Console.WriteLine("warning: match not found");
      return 0;
    } else if (filtered.Count == 1) {
      return impl(filtered[0]);
    } else {
      var allow_wildcards = test.Contains("*") || test.Contains("?");
      if (allow_wildcards) {
        // todo. don't swallow exit codes
        filtered.ForEach(filtered0 => impl(filtered0));
        Console.WriteLine("{0} file{1} removed", filtered.Count, filtered.Count != 1 ? "s" : "");
        return 0;
      } else {
        Console.WriteLine("error: match is ambiguous");
        filtered.Take(5).ToList().ForEach(file => Console.WriteLine("    " + file));
        if (filtered.Count > 5) Console.WriteLine("    ... " + (filtered.Count - 5) + " more");
        return -1;
      }
    }
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }

  public static bool MatchesWildcard(this String s, String wildcard) {
    return Operators.LikeString(s, "*" + wildcard + "*", CompareMethod.Text);
  }
}
