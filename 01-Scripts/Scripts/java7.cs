// build this with "csc /t:winexe java7.cs"

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Java6 {
  public static void Main(String[] args) {
    FS.Mklink(@"c:\PROGRA~1\Java\JDK17~1.0_0", @"c:\PROGRA~1\Java\jdk");
    FS.Mklink(@"c:\PROGRA~1\Java\jre7", @"c:\PROGRA~1\Java\jre");
  }
}

public static class FS {
  public static void Mklink(String from, String to) {
    var psi = new ProcessStartInfo();
    var mklink = @"%SCRIPTS_HOME%\deploy-symlink.exe".Expand();
    psi.FileName = mklink;
    psi.Arguments = from + " " + to;
    psi.UseShellExecute = false;
    psi.CreateNoWindow = true;
    Process.Start(psi).WaitForExit();
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}
