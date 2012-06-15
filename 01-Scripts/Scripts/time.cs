using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class App {
  public static int Main(String[] args) {
    if (args.Length == 0) {
      Console.WriteLine("usage: time <command>");
      return -1;
    }

    var sw = new Stopwatch();
    sw.Start();
    var cmd = String.Join(" ", args);
    var status = exec(cmd);

    sw.Stop();
    var ts = sw.Elapsed;
    Console.WriteLine("Elapsed: " + ts);
    return status;
  }

  private static int exec(String command, DirectoryInfo home = null) {
    var script = Path.GetTempFileName() + ".bat";
    File.AppendAllText(script, "@echo off" + "\r\n");
    File.AppendAllText(script, "cd /D \"" + (home ?? new DirectoryInfo(".")).FullName + "\"" + "\r\n");
    File.AppendAllText(script, command + "\r\n");
    File.AppendAllText(script, "exit /b %errorlevel%");

    var psi = new ProcessStartInfo();
    psi.FileName = script;
    psi.WorkingDirectory = (home ?? new DirectoryInfo(".")).FullName;
    psi.UseShellExecute = false;

    var p = new Process();
    p.StartInfo = psi;

    if (p.Start()) {
      p.WaitForExit();
      return p.ExitCode;
    } else {
      return -1;
    }
  }
}