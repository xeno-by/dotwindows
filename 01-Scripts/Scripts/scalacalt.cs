using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Type {
  public static void Main(String[] args) {
    var process = new Process();
    var myke = @"%SCRIPTS_HOME%\myke.exe".Expand();
    process.StartInfo.FileName = myke;
    process.StartInfo.Arguments = "compile-alt /C:scala " + String.Join(" ", args);
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
