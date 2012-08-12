// build this with "csc /t:winexe difflast.cs"

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
      var traceDir = new DirectoryInfo(@"%HOME%\.myke_important".Expand());
      if (traceDir.Exists) {
        var logs = traceDir.GetFiles("*.log").OrderByDescending(fi => fi.LastWriteTime).ToList();
        var last = logs.Count() > 0 ? logs[0].FullName : null;
        var prelast = logs.Count() > 1 ? logs[1].FullName : null;
        if (logs.Count() > 1) {
          var mydiff = @"C:\Program Files (x86)\scripts\mydiff.exe";
          Process.Start(mydiff, last + " " + prelast);
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