// build this with "csc /t:winexe tgit.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var cmd = args.ElementAtOrDefault(0);
    if (cmd == null) cmd = "log";
    cmd = String.Format("/command:\"{0}\"", cmd);
   
    var path = args.ElementAtOrDefault(1);
    if (path == null) path = ".";
    path = String.Format("/path:\"{0}\"", path);

    var tgit = @"C:\Program Files\TortoiseGit\bin\TortoiseProc.exe";
    Process.Start(tgit, String.Join(",", new []{cmd, path}));
  }
}