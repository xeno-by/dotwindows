using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices;

public static class OS {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ProcessBasicInformation {
    internal IntPtr Reserved1;
    internal IntPtr PebBaseAddress;
    internal IntPtr Reserved2_0;
    internal IntPtr Reserved2_1;
    internal IntPtr UniqueProcessId;
    internal IntPtr InheritedFromUniqueProcessId;
  }

  [DllImport("ntdll.dll")]
  private static extern int NtQueryInformationProcess(
    IntPtr processHandle,
    int processInformationClass,
    ref ProcessBasicInformation processInformation,
    int processInformationLength,
    out int returnLength);

  public static int ParentId(this Process process) {
    var pbi = new ProcessBasicInformation();
    int returnLength;
    int status = NtQueryInformationProcess(process.Handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
    if (status != 0) throw new Exception("ParentId has failed with status: " + status);
    return pbi.InheritedFromUniqueProcessId.ToInt32();
  }

  public static void KillTree(this Process process) {
    if (process != null) {
      process.Kill();
      process.WaitForExit();

      Process.GetProcesses().ToList().ForEach(p => {
        try {
          if (p.ParentId() == process.Id) {
            p.Kill();
            p.WaitForExit();
          }
        } catch {
          // do nothing
        }
      });
    }
  }

  public static bool IsPortAvailable(this int port) {
    var conns = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
    return !conns.Any(conn => conn.Port == port);
  }
}

public class App {
  public static void Main(String[] args) {
    Process ensime = LaunchEnsime();
    Process adapter = null;

    try {
      var portfile = ensime.StartInfo.Arguments;
      while (!File.Exists(portfile) || new FileInfo(portfile).Length == 0) Thread.Sleep(1000);
      var adaptee = int.Parse(File.ReadAllText(portfile));
      var adapted = adaptee + 1;
      while (!adapted.IsPortAvailable()) adapted++;
      adapter = LaunchAdapter(adaptee, adapted);
    } catch {
      ensime.KillTree();
      adapter.KillTree();
      throw;
    }

    ensime.WaitForExit();
    adapter.KillTree();
  }

  private static Process LaunchEnsime() {
    var psi = new ProcessStartInfo();
    psi.FileName = @"%ENSIME_HOME%\bin\server.bat".Expand();
    psi.WorkingDirectory = @"%ENSIME_HOME%".Expand();
    psi.Arguments = Path.GetTempFileName();
    psi.UseShellExecute = false;

    var p = new Process();
    p.StartInfo = psi;

    if (p.Start()) {
      return p;
    } else {
      return null;
    }
  }

  private static Process LaunchAdapter(int adaptee, int adapted) {
    var temp = Path.GetTempFileName() + ".ensimea.exe";
    File.Copy(@"%SCRIPTS_HOME%\ensimea.exe".Expand(), temp);

    var psi = new ProcessStartInfo();
    psi.FileName = temp;
    psi.Arguments = String.Format("{0} {1}", adaptee, adapted);
    psi.UseShellExecute = false;
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardError = true;

    var p = new Process();
    p.StartInfo = psi;
    p.OutputDataReceived += (sender, args) => { if (args.Data != null) Console.WriteLine(args.Data); };
    p.ErrorDataReceived += (sender, args) => { if (args.Data != null) Console.WriteLine(args.Data); };

    if (p.Start()) {
      p.BeginOutputReadLine();
      p.BeginErrorReadLine();
      return p;
    } else {
      return null;
    }
  }
}