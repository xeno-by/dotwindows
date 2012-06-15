\// build this with "csc mykill.cs"

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

public class App {
  public static int Main(String[] args) {
    println("mykill " + String.Join(" ", args));
    var tree = args.Contains("/tree");
    args = args.Where(arg => !arg.StartsWith("/")).ToArray();

    String s_pid = null;
    if (args.Length == 0) {
      Console.Write("Pid: ");
      s_pid = Console.ReadLine();
    } else {
      s_pid = args[0];
    }

    int pid;
    if (int.TryParse(s_pid, out pid)) {
      try {
        var p = Process.GetProcessById(pid);
        Kill(p, tree);
        return 0;
      } catch(Exception ex) {
        println(ex.Message);
        return -1;
      }
    } else {
      return -1;
    }
  }

  private static void print(Object o) {
    File.AppendAllText(@"C:\Program Files (x86)\scripts\mykill.log", o.ToString());
    Console.WriteLine(o);
  }

  private static void println(Object o) {
    print(o + "\r\n");
  }

  private static void Kill(Process p, Boolean tree) {
    try {
      print(String.Format("Process {0} {1}", p.Id, p.ProcessName));

      try {
        var commandLine = "";
        using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + p.Id))
          foreach (ManagementObject mo in mos.Get()) { commandLine += mo["CommandLine"]; }
        println(": " + commandLine);
      } catch {
        println("");
        throw;
      }

      try {
        p.Kill();
      } catch {
        // ignore
      }

      if (tree) {
        var children =
          from ManagementBaseObject m in new ManagementObjectSearcher("select ProcessId from Win32_Process where ParentProcessId=" + p.Id).Get()
          select Process.GetProcessById(Convert.ToInt32(m["ProcessId"]));
        children.ToList().ForEach(child => Kill(child, tree));
      }
    } catch {
      // ignore
    }
  }
}