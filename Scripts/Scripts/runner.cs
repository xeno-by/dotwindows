using System;
using System.Linq;
using System.Diagnostics;

public class runner {
  [STAThread]
  public static void Main(String[] args) {
    if (args.Length == 0) return;
    
    var dir = Environment.CurrentDirectory;
    if (args[0].StartsWith("/dir:")) { dir = args[0].Substring("/dir:".Length); args = args.Skip(1).ToArray(); }
    if (args.Length == 0) return;

    var process = new Process();
    process.StartInfo.WorkingDirectory = dir;
    process.StartInfo.FileName = args[0];
    process.StartInfo.Arguments = String.Join(" ", args.Skip(1).ToArray());
    process.Start();
  }
}