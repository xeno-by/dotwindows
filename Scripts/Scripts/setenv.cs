using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public class App
{
  public static int Main(String[] args) {
    var asm = Assembly.GetEntryAssembly();
    var dir = Path.GetDirectoryName(asm.Location);
    var target = dir + @"\setenvcore.exe";
    if (args.Length > 3) args = new []{args[0], args[1], String.Join(" ", args.Skip(2))};
    if (args[2].Contains(" ")) args[2] = "\"" + args[2];
    var cmdline = String.Join(" ", args);
    Console.Write("setenv " + cmdline);

    var psi = new ProcessStartInfo(target, cmdline);
    psi.CreateNoWindow = true;
    psi.UseShellExecute = false;
    psi.ErrorDialog = false;
    psi.RedirectStandardOutput = true;
    var p = Process.Start(psi);
 
    var output = p.StandardOutput;
    p.WaitForExit();
    Console.WriteLine(output.ReadToEnd());
    return p.ExitCode;
  }
}