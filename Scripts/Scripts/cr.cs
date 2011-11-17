// build this with "csc /t:winexe cr.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var cmd = "commit";
    cmd = String.Format("/command:\"{0}\"", cmd);
   
    var path = @"c:\Projects\Reflection";
    path = String.Format("/path:\"{0}\"", path);

    var tgit = @"C:\Program Files\TortoiseGit\bin\TortoiseProc.exe";
    Process.Start(tgit, String.Join(",", new []{cmd, path}));
  }
}