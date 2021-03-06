// build this with "csc /t:winexe /r:System.Windows.Forms.dll mymerge.cs"

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

public class App {
  [STAThread]
  public static int Main(String[] args) {
    if (args.Length != 4) return -1;
    var @base = args[0];
    var mine = args[1];
    var theirs = args[2];
    var merged = args[3];

    var kdiff3 = @"C:\Program Files (x86)\Kdiff3\Kdiff3.exe";
    var p = Process.Start(kdiff3, String.Join(" ", new []{@base, mine, theirs, "-o", merged, "-L1", "Base", "-L2", "Mine", "-L3", "Merged"}));
    //var winmerge = @"C:\Program Files (x86)\WinMerge\WinMergeU.exe";
    //var p = Process.Start(winmerge, String.Join(" ", new []{"-dl", "Mine", "-dr", "Base", mine, @base}));

    p.WaitForExit();
    return p.ExitCode;
  }
}