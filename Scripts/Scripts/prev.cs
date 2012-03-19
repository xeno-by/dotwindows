// build this with "csc /t:winexe prev.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var traceDir = new DirectoryInfo(@"%HOME%\.myke".Expand());
    if (traceDir.Exists) {
      var logs = traceDir.GetFiles("*.log").OrderByDescending(fi => fi.LastWriteTime).ToList();
      if (logs.Count() > 0) {
        var config = new FileInfo(traceDir + "\\" + ".config");
        var prevprev = config.Exists ? new FileInfo(config.ReadAllText()) : null;
        if (prevprev != null && !prevprev.Exists) prevprev = null;
        if (prevprev != null && config.LastWriteTime < logs[0].LastWriteTime) prevprev = null;

        logs = logs.SkipWhile(log => prevprev != null && log.FullName != prevprev.FullName).ToList();
        if (prevprev != null) logs = logs.Skip(1).ToList();
        var prev = logs.Count() > 0 ? logs[0].FullName : null;
        config.WriteAllText(prev ?? "");

        if (prev != null) {
          var sublime = @"C:\Program Files (x86)\scripts\sublime.exe";
          Process.Start(sublime, prev);
        }
      }
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