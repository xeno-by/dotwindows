using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var psi = new ProcessStartInfo();
    psi.CreateNoWindow = true;
    psi.WindowStyle = ProcessWindowStyle.Hidden;
    psi.UseShellExecute = false;
    psi.FileName = @"%SCRIPTS_HOME%\ant.exe".Expand();
    psi.Arguments = "/maxi /reg";
    Process.Start(psi);
  }
}


public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}
