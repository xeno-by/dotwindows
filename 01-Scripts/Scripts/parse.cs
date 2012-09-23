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

    code = code.Trim();

    var file = Path.GetTempFileName() + ".scala";
    if (!code.StartsWith("class") && !code.StartsWith("object") && !code.StartsWith("trait")) code = "object wrapper { " + code + " }";
    File.WriteAllText(file, code);
    var dir = Path.GetDirectoryName(file);
    var name = Path.GetFileName(file);

    var process = new Process();
    var scala = @"%SCRIPTS_HOME%\scalac.exe".Expand();
    process.StartInfo.FileName = scala;
    process.StartInfo.WorkingDirectory = dir;
    process.StartInfo.Arguments = "-language:experimental.macros -Xprint:parser -Yshow-trees -Yshow-trees-compact -Yshow-trees-stringified -Ystop-after:parser " + name;
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
