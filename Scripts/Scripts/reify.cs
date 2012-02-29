using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Reify {
  public static void Main(String[] args) {
    String code = null;
    if (args.Length == 0) {
      Console.Write("Lift: ");
      code = Console.ReadLine();
    } else {
      if (args.Length == 1 && File.Exists(args[0])) {
        code = File.ReadAllText(args[0]);
      } else {
        code = String.Join(" ", args);
      }
    }

    var temp = Path.GetTempFileName();
    File.WriteAllText(temp, "object __wrapper { def wrapper() = scala.reflect.Code.lift{" + code + "} }");

    var process = new Process();
    var scala = @"%SCRIPTS_HOME%\scalac.exe".Expand();
    process.StartInfo.FileName = scala;
    process.StartInfo.Arguments = "-Yreify-copypaste " + temp;
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
