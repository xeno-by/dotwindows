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
      var sandbox = new DirectoryInfo(@"%PROJECTS%\Kepler\sandbox\".Expand());
      var staged = sandbox.Exists ? sandbox.GetFiles("*.scala", SearchOption.AllDirectories).Select(file => file.FullName.Substring(sandbox.FullName.Length)).ToList() : new List<String>();
      Console.WriteLine("{0} file{1} staged", staged.Count, staged.Count != 1 ? "s" : "");
      staged.ForEach(file => Console.WriteLine("    " + file));
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
      Console.WriteLine("staging: " + filtered[0]);
      test = filtered[0];

      var sandbox = new DirectoryInfo(@"%PROJECTS%\Kepler\sandbox\".Expand());
      if (!sandbox.Exists) sandbox.Create();
      var from = new FileInfo(root.FullName + test);
      var to = new FileInfo(sandbox.FullName + Path.GetFileName(test));

      var process = new Process();
      var deploySymlink = @"%SCRIPTS_HOME%\deploy-symlink.exe".Expand();
      process.StartInfo.FileName = deploySymlink;
      process.StartInfo.Arguments = String.Format("\"{0}\" \"{1}\"", from.FullName, to.FullName);
      process.StartInfo.UseShellExecute = false;
      process.Start();
      process.WaitForExit();
      if (process.ExitCode != 0) return process.ExitCode;

      process = new Process();
      var addtest = @"%SCRIPTS_HOME%\test.exe".Expand();
      process.StartInfo.FileName = addtest;
      process.StartInfo.Arguments = String.Format("\"{0}\"", test);
      process.StartInfo.UseShellExecute = false;
      process.Start();
      process.WaitForExit();
      return process.ExitCode;
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
