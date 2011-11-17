// build this with "csc /t:winexe ca.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var cmd = "commit";
    cmd = String.Format("/command:\"{0}\"", cmd);
   
    var path = @"D:\Dropbox\Projects\Advanced Algorithms";
    path = String.Format("/path:\"{0}\"", path);

    var tgit = @"C:\Program Files\TortoiseGit\bin\TortoiseProc.exe";
    Process.Start(tgit, String.Join(",", new []{cmd, path}));
  }
}