using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Parse {
  public static void Main(String[] args) {
    String code = null;
    if (args.Length == 0) {
      Console.Write("Parse: ");
      code = Console.ReadLine();
    } else {
      if (args.Length == 1 && File.Exists(args[0])) {
        code = File.ReadAllText(args[0]);
      } else {
        code = String.Join(" ", args);
      }
    }

    var file = Path.GetTempFileName();
    if (!code.Contains("class") && !code.Contains("object")) code = "object wrapper { " + code + " }";
    File.WriteAllText(file, code);

    var process = new Process();
    var scala = @"%SCRIPTS_HOME%\scalac.bat".Expand();
    process.StartInfo.FileName = scala;
    process.StartInfo.Arguments = "-Xprint:parser -Yshow-trees -Ystop-after:parser \"" + file + "\"";
    process.StartInfo.UseShellExecute = false;
    process.Start();
    process.WaitForExit();
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}
