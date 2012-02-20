using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Type {
  public static void Main(String[] args) {
    String code = null;
    if (args.Length == 0) {
      Console.Write("Type: ");
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
    var scala = @"%SCRIPTS_HOME%\scalac.exe".Expand();
    process.StartInfo.FileName = scala;
    process.StartInfo.Arguments = "-Xprint:typer -Yshow-trees -Xprint-types -Ystop-after:typer \"" + file + "\"";
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
