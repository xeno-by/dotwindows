// build this with "csc ctrlc.cs"

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;

public class App {
  [DllImport("kernel32.dll", SetLastError=true)]
  static extern bool GenerateConsoleCtrlEvent(int sigevent, int dwProcessGroupId);

  public static int Main(String[] args) {
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
        Console.WriteLine(String.Format("Process {0} {1} (session id: {2})", p.Id, p.ProcessName, p.SessionId));
        if (GenerateConsoleCtrlEvent(0, pid)) {
          return 0;
        } else {
          var err = Marshal.GetLastWin32Error();
          Console.WriteLine("Error " + err + " (" + new Win32Exception(err).Message + ")");
          return -1;
        }
      } catch(Exception ex) {
        Console.WriteLine(ex.Message);
        return -1;
      }
    } else {
      return -1;
    }
  }
}