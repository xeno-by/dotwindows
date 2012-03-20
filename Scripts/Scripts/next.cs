// build this with "csc /t:winexe next.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    try {
      var traceDir = new DirectoryInfo(@"%HOME%\.myke".Expand());
      if (traceDir.Exists) {
        var logs = traceDir.GetFiles("*.log").OrderByDescending(fi => fi.LastWriteTime).ToList();
        if (logs.Count() > 0) {
          var config = new FileInfo(traceDir + "\\" + ".config");
          var prevprev = config.Exists ? new FileInfo(config.ReadAllText()) : null;
          if (prevprev != null && !prevprev.Exists) prevprev = null;
          if (prevprev != null && config.LastWriteTime < logs[0].LastWriteTime) prevprev = null;

          logs = logs.TakeWhile(log => prevprev != null && log.FullName != prevprev.FullName).ToList();
          if (prevprev == null) logs = logs.Take(1).ToList();
          var next = logs.Count() > 0 ? logs.Last().FullName : null;
          config.WriteAllText(next ?? "");

          if (next != null) {
            var sublime = @"C:\Program Files (x86)\scripts\sublime.exe";
            Process.Start(sublime, next);
          } else {
            var last = @"C:\Program Files (x86)\scripts\last.exe";
            Process.Start(last);
          }
        }
      }
    } catch (Exception ex) {
      MessageBox.Show(ex.ToString());
    }
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}

public static class IO {
  public static String ReadAllText(this FileInfo file) {
    return File.ReadAllText(file.FullName);
  }

  public static List<String> ReadAllLines(this FileInfo file) {
    return File.ReadAllLines(file.FullName).ToList();
  }

  public static void WriteAllText(this FileInfo file, String s) {
    File.WriteAllText(file.FullName, s);
  }

  public static void WriteAllLines(this FileInfo file, IEnumerable<String> lines) {
    File.WriteAllText(file.FullName, String.Join(Environment.NewLine, lines.ToArray()));
  }
}