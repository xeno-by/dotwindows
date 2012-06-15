using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

public class App {
  public static int Main(String[] args) {
    var psi = new ProcessStartInfo();
    psi.FileName = "cmd.exe";
//    psi.Arguments = String.Join(" ", args);
    psi.Arguments = "/C cmd.exe 2>&1";
    psi.WorkingDirectory = Directory.GetCurrentDirectory();
    psi.UseShellExecute = false;
    psi.RedirectStandardInput = true;
    psi.RedirectStandardOutput = true;
    // NB: when you don't redirect standard error, it gets merged into standard output
//    psi.RedirectStandardError = true;

    var p = new Process();
    p.StartInfo = psi;
    p.Exited += (o, e) => Environment.Exit(p.ExitCode);

    if (p.Start()) {
      var stdout = new StandardOutputReader(p);
//      var stderr = new StandardErrorReader(p);

      while (!p.HasExited) {
        WaitHandle.WaitAll(new []{stdout.Blocked});
//        WaitHandle.WaitAll(new []{stdout.Blocked, stderr.Blocked});

        var flag = new AutoResetEvent(false);
        var waiter = new Thread(() => { WaitHandle.WaitAny(new []{stdout.Unblocked}); flag.Set(); });
//        var waiter = new Thread(() => { WaitHandle.WaitAny(new []{stdout.Unblocked, stderr.Unblocked}); flag.Set(); });
        var writer = new Thread(() => {
          var command = Console.ReadLine();
          HandleCommand(p, command);
        });

        waiter.Start();
        writer.Start();
        flag.WaitOne();
      }

      return p.ExitCode;
    } else {
      return -1;
    }
  }

  private static String LastCommand = null;
  private static void HandleCommand(Process p, String command) {
    var s_macros = File.ReadAllLines(@"%SCRIPTS_HOME%\macros.doskey".Expand()).Where(line => !line.StartsWith(";=") && line.Trim() != String.Empty).ToList();
    var macros = s_macros.ToDictionary(s => s.Substring(0, s.IndexOf("=")), s => s.Substring(s.IndexOf("=") + 1));

    foreach (var macro_key in macros.Keys) {
      var macro_value = macros[macro_key];
      if (command.StartsWith(macro_key)) {
        if (command == macro_key) {
          command = macro_value.DoskeyMacroSub(command, "");
          break;
        } else {
          if (command.StartsWith(macro_key + " ")) {
            command = macro_value.DoskeyMacroSub(command, command.Substring((macro_key + " ").Length));
            break;
          }
        }
      }
    }

    LastCommand = command + Environment.NewLine;
    p.StandardInput.WriteLine(command);
    p.StandardInput.Flush();
  }

  private abstract class StandardStreamReader {
    public StreamReader Stream;
    public EventWaitHandle Blocked = new ManualResetEvent(false);
    public EventWaitHandle Unblocked = new ManualResetEvent(true);

    public StandardStreamReader(Process p, StreamReader stream) {
      this.Stream = stream;
      var lk = new Object();
      EventWaitHandle finishedReading = new AutoResetEvent(false);
      var reads = 0;

      var reader = new Thread(() => {
        try {
          while (!p.HasExited) {
            const int cnt = 1;
            var buf = new char[cnt];
            var n = TryRead(buf, 0, cnt);
            finishedReading.Set();
            if (n == 0) break;
            lock (lk) { reads++; }
            AfterChunkRead(new String(buf, 0, n));
          }
        } catch {
          var old = finishedReading;
          finishedReading = new ManualResetEvent(true);
          old.Set();
        }
      });

      var poller = new Thread(() => {
        while (!p.HasExited) {
          var success = finishedReading.WaitOne(50);
          if (!success) {
            Blocked.Set();
            Unblocked.Reset();
            finishedReading.WaitOne();
            Unblocked.Set();
            Blocked.Reset();
          }
        }
      });

      reader.Start();
      poller.Start();
    }

    protected abstract int TryRead(Char[] buf, int start, int len);
    protected abstract void AfterChunkRead(String s);
  }

  private class StandardOutputReader: StandardStreamReader {
    public StandardOutputReader(Process p): base(p, p.StandardOutput) {}

    protected override int TryRead(Char[] buf, int start, int len) {
      return Stream.Read(buf, start, len);
    }

    private String myLastCommand = null;
    private int charsToIgnore = 0;
    protected override void AfterChunkRead(String s) {
      if (myLastCommand != LastCommand) {
        charsToIgnore = LastCommand == null ? 0 : LastCommand.Length;
        myLastCommand = LastCommand;
      }

      if (charsToIgnore > 0) {
        var ignore = charsToIgnore;
        if (s.Length < ignore) ignore = s.Length;
        charsToIgnore -= ignore;
        s = s.Substring(ignore);
      }

      Console.Write(s);
    }
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }

  // todo. implement more doskey stuff as it becomes necessary
  public static String DoskeyMacroSub(this String macro, String command, String args) {
    return macro.Replace("$*", args);
  }
}