// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using LibGit2Sharp;
using ZetaLongPaths;

public class App {
  public static int Main(String[] args) {
    var path = @"Software\Far2\KeyMacros\Vars";
    var reg = Registry.CurrentUser.OpenSubKey(path, true) ?? Registry.CurrentUser.CreateSubKey(path);
    var env_values = reg.GetValueNames().Where(name => name.StartsWith("%%Myke")).ToList();
    env_values.ForEach(value => { try { reg.DeleteValue(value); } catch { /* ignore */ } });
    // System.Console.WriteLine("launching myke: " + String.Join(" ", args));

    try {
      try {
        var status = MainWrapper(args);
        Environment.ExitCode = status.value;
      } catch {
        Environment.ExitCode = -1;
        throw;
      }
    } finally {
      Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%MykeStatus", Environment.ExitCode.ToString());
      Func<String, String> capitalize = s => String.IsNullOrEmpty(s) ? s : Char.ToUpper(s[0]) + s.Substring(1);
      if (Config.env != null) Config.env.Keys.ToList().ForEach(key => Registry.SetValue(@"HKEY_CURRENT_USER\Software\Far2\KeyMacros\Vars", "%%Myke" + capitalize(key), Config.env[key]));
    }

    return Environment.ExitCode;
  }

  private static ExitCode MainWrapper(String[] args) {
    var status = Config.parse(args);
    if (status != 0) return status;

    if (Config.verbose) {
      Config.print();
    }

    var originalTarget = Config.target;
    var t_all = Connectors.all;
    if (Config.verbose) {
      // Console.println();
      // Console.println("connectors: ");
      // Help.printConnectors();
    }

    while (Config.target != null) {
      if (Config.verbose) {
        Console.println();
        Console.println("considering target {0}", Config.target);
      }

      foreach (var t_conn in t_all) {
        if (Config.verbose) {
          Console.print("* {0}... ", t_conn.name());
        }

        Func<Conn> loadConn = () => {
          try {
            var myconn = (Conn)t_conn.instantiate();
            var accepts = myconn != null && myconn.accept() && (Config.requestedConn == null || Config.requestedConn == myconn.name().ToUpper());
            if (Config.verbose) {
              if (accepts) Console.println("[A]");
              else Console.println("[R]");
            }
            return accepts ? myconn : null;
          } catch (Exception ex) {
            if (Config.verbose) {
              Console.println("[R]");
              Console.println(ex);
            }
            return null;
          }
        };
        var conn = loadConn();

        if (conn != null) {
          Config.conn = conn;
          Config.env["traceFile"] = Trace.primaryFileName(conn.name(), Config.action);

          var action = Config.action;
          if (Config.verbose) {
            Console.println();
            Console.println("========================================");
            Console.println("{0} will now perform {1} upon {2}", conn.name(), Config.action, Config.target);
            Console.println("========================================");
            Console.println();
          }

          var actions = conn.actions();
          if (!actions.ContainsKey(action)) {
            Config.env["traceFile"] = Trace.auxiliaryFileName(conn.name(), Config.action);
            Console.println("error: {0} does not know how to do {1}", conn.name(), Config.action);
            return -1;
          }

          ExitCode exitCode = -1;
          try {
            Config.conn_action = conn.GetType().actions()[action];
            conn.init();
            exitCode = (ExitCode)actions[action]();
            return exitCode;
          } finally {
            var env = Config.env;
            if (!env.ContainsKey("action")) env["action"] = action;
            if (!env.ContainsKey("target")) env["target"] = Config.target;
            if (!env.ContainsKey("args")) env["args"] = Config.args.ToString();
            if (!env.ContainsKey("root")) env["root"] = conn.root == null ? null : conn.root.ToString();
            if (!env.ContainsKey("meaningful")) {
              env["meaningful"] = (exitCode ? 0 : 1).ToString();
              if (Config.conn_action.meaningful()) env["meaningful"] = "1";
            }
          }
        }
      }

      try { Config.target = Path.GetDirectoryName(Config.target); }
      catch { Config.target = null; }
    }

    if (Config.verbose) {
      Console.println();
    }

    Console.println("error: no connector accepted the target {0}", originalTarget);

    return -1;
  }
}

public class ExitCode {
  public int value;

  public static ExitCode operator !(ExitCode c) {
    return c.value == 0 ? new ExitCode{value = -1} : new ExitCode{value = 0};
  }

  public static bool operator ==(ExitCode c1, ExitCode c2) {
    return Equals(c1, c2);
  }

  public static bool operator !=(ExitCode c1, ExitCode c2) {
    return !Equals(c1, c2);
  }

  public override bool Equals(Object o_other) {
    var other = o_other as ExitCode;
    if (other == null) return false;
    return this.value == other.value;
  }

  public override int GetHashCode() {
    return value;
  }

  public override string ToString() {
    return value.ToString();
  }

  public static implicit operator ExitCode(bool value) {
    return new ExitCode { value = value ? 0 : -1 };
  }

  public static implicit operator ExitCode(int value) {
    return new ExitCode { value = value };
  }

  public static ExitCode operator &(ExitCode c1, ExitCode c2) {
    return c1 ? c2 : 0;
  }

  public static ExitCode operator |(ExitCode c1, ExitCode c2) {
    return c1 ? 0 : c2;
  }

  public static bool operator true(ExitCode c) {
    return c.value == 0;
  }

  public static bool operator false(ExitCode c) {
    return c.value != 0;
  }

  public static implicit operator bool(ExitCode c) {
    return c.value == 0;
  }
}

public static class Console {
  public static void print(String format, params Object[] objs) {
    trace(format, objs);
    System.Console.Write(String.Format(format, objs));
  }

  public static void print(String obj) {
    trace(obj);
    System.Console.Write(obj);
  }

  public static void print(Object obj) {
    trace(obj);
    System.Console.Write(obj);
  }

  public static void println(String format, params Object[] objs) {
    traceln(format, objs);
    System.Console.WriteLine(String.Format(format, objs));
  }

  public static void println(String obj) {
    traceln(obj);
    System.Console.WriteLine(obj);
  }

  public static void println(Object obj) {
    traceln(obj);
    System.Console.WriteLine(obj);
  }

  public static void println() {
    traceln();
    System.Console.WriteLine();
  }

  public static bool firstTrace = true;
  public static void internalTrace(String msg) {
    if (firstTrace) {
      firstTrace = false;
      traceln("myke {0} {1} {2}", Config.action, Config.originalTarget == null ? null : Config.originalTarget.ShellEscape(), Config.args);
      if (Config.conn is Git) {
        var git = Config.conn as Git;
        var backup = Config.env.ContainsKey("workingDir") ? Config.env["workingDir"] : null;
        traceln("git: branch = {0}, commit = {1}", git.getCurrentBranch(), git.getCurrentCommit());
        if (backup != null) Config.env["workingDir"] = backup;
      }
    }

    var traceToPrimary = (Config.conn_action == null || Config.conn_action.canTrace()) && !Config.env.ContainsKey("dontTrace");
    if (!traceToPrimary) Config.env["traceFile"] = Trace.auxiliaryFileName(Config.env["traceFile"]);
    File.AppendAllText(Config.env["traceFile"], msg);
  }

  public static void trace(String format, params Object[] objs) {
    internalTrace(String.Format(format, objs));
  }

  public static void trace(String obj) {
    internalTrace(obj);
  }

  public static void trace(Object obj) {
    internalTrace(obj == null ? "" : obj.ToString());
  }

  public static void traceln(String format, params Object[] objs) {
    trace(format, objs);
    trace(Environment.NewLine);
  }

  public static void traceln(String obj) {
    trace(obj);
    trace(Environment.NewLine);
  }

  public static void traceln(Object obj) {
    trace(obj);
    trace(Environment.NewLine);
  }

  public static void traceln() {
    trace(Environment.NewLine);
  }

  public class Point {
    public int x;
    public int y;

    public Point(int x, int y) {
      this.x = x;
      this.y = y;
    }

    public Point(Point other) {
      this.x = other.x;
      this.y = other.y;
    }

    public override String ToString() {
      return String.Format("x = {0}, y = {0}", x, y);
    }
  }

  public static String readln(String prompt = null, String history = null) {
    if (history != null) {
      var key = @"Software\Myke\" + history.Replace("\\", "$slash$");
      var reg = Registry.CurrentUser.OpenSubKey(key, true) ?? Registry.CurrentUser.CreateSubKey(key);

      var indices = reg.GetValueNames().Where(name => name.StartsWith("arguments")).Select(name => {
        int index;
        return int.TryParse(name.Substring("arguments".Length), out index) ? index: -1;
      }).OrderBy(index => index).Where(index => index != -1).ToList();
      var maxIndex = indices.Count == 0 ? 0 : indices.Max();
      var fullList = indices.Select(index => reg.GetValue("arguments" + index).ToString()).ToList();
      var shortList = indices.Select(index => reg.GetValue("arguments" + index).ToString()).Distinct().ToList();

      var @default = fullList.LastOrDefault() ?? String.Empty;
      if (!String.IsNullOrEmpty(prompt)) {
        print(prompt);
        if (!String.IsNullOrEmpty(@default)) print(" (default is {0})", @default);
        print(": ");
      };

      var input = ((Func<String>)(() => {
        var dumb = ((Func<bool>)(() => { try { var aux = System.Console.KeyAvailable; return false; } catch { return true; } }))();
        if (dumb) {
          return System.Console.ReadLine();
        } else {
          var logfile = ((Func<String>)(() => {
            if (File.Exists(Config.originalTarget)) return Config.originalTarget + ".log";
            if (Directory.Exists(Config.originalTarget)) return Config.originalTarget + ".log";
            return null;
          }))();

          if (logfile != null && File.Exists(logfile)) File.Delete(logfile);
          Action<String> log = msg => {
            if (Config.verbose && logfile != null) {
              File.AppendAllText(logfile, msg + "\r\n");
            }
          };

          var buf = String.Empty;
          var oldbuf = buf;
          var historypos = shortList.Count;
          Action dechistorypos = () => { if (historypos > 0) historypos--; };
          Action inchistorypos = () => { if (historypos < shortList.Count) historypos++; };
          Func<String> currenthistory = () => historypos == shortList.Count ? "" : shortList[historypos];
          var pos = new Point(System.Console.CursorLeft, System.Console.CursorTop);
          var origpos = new Point(System.Console.CursorLeft, System.Console.CursorTop);
          log(String.Format("original pos: x = {0}, y = {1}", origpos.x, origpos.y));
          Func<int> currentpos = () => (pos.y - origpos.y) * System.Console.WindowWidth + (pos.x - origpos.x);
          Func<Char> currentchar = () => currentpos() >= buf.Length ? '\0' : buf[currentpos()];
          Action<int> gotopos = null;
          gotopos = idx => {
            if (idx < 0) gotopos(0);
            else if (idx > buf.Length) gotopos(buf.Length);
            else {
              log(String.Format("going to {0}", idx));
              log(String.Format("currentpos: {0}", currentpos()));
              log(String.Format("pos is now {0}", pos));
              pos = new Point(origpos);
              pos.x += idx;
              while (pos.x > System.Console.WindowWidth) {
                pos.x -= System.Console.WindowWidth;
                pos.y += 1;
              }
              log(String.Format("currentpos: {0}", currentpos()));
              log(String.Format("pos is now {0}", pos));
            }
          };
          Action decpos = () => gotopos(currentpos() - 1);
          Action incpos = () => gotopos(currentpos() + 1);
          Action redraw = () => {
            System.Console.SetCursorPosition(origpos.x, origpos.y);
            System.Console.Write(new String(' ', oldbuf.Length));
            System.Console.SetCursorPosition(origpos.x, origpos.y);
            System.Console.Write(buf);
            oldbuf = buf;
            System.Console.SetCursorPosition(pos.x, pos.y);
          };

          while (true) {
            log("");
            log("===readkey===");
            log(String.Format("buf = {0}, currentpos = {1}", buf, currentpos()));
            log(String.Format("[before] pos: x = {0}, y = {1}", System.Console.CursorLeft, System.Console.CursorTop));
            var kp = System.Console.ReadKey();
            log(String.Format("key = {0}, char = {1}, modifiers = {2}", kp.Key, ((int)kp.KeyChar).ToString("x4"), kp.Modifiers));
            log(String.Format("[after] pos: x = {0}, y = {1}", System.Console.CursorLeft, System.Console.CursorTop));

            if (kp.KeyChar != '\0') {
              if (kp.KeyChar == 0x0d) {
                println();
                break;
              } else if (kp.KeyChar == 0x08) { // backspace
                if (currentpos() > 0) {
                  var buf1 = new StringBuilder();
                  if (currentpos() > 0 && buf.Length > 1) buf1.Append(buf.Substring(0, currentpos() - 1));
                  if (currentpos() < buf.Length) buf1.Append(buf.Substring(currentpos(), buf.Length - currentpos()));
                  buf = buf1.ToString();
                  decpos();
                }
              } else if (kp.KeyChar == 0x1b) { // escape
                gotopos(0);
                buf = "";
              } else if (kp.KeyChar < 0x20) {
                // do nothing
              } else {
                var buf1 = new StringBuilder();
                if (currentpos() > 0) buf1.Append(buf.Substring(0, currentpos()));
                buf1.Append(kp.KeyChar);
                if (currentpos() < buf.Length) buf1.Append(buf.Substring(currentpos(), buf.Length - currentpos()));
                buf = buf1.ToString();
                gotopos(currentpos() + 1);
              }
            } else {
              if (kp.Key == ConsoleKey.Home) {
                gotopos(0);
              } else if (kp.Key == ConsoleKey.End) {
                gotopos(buf.Length);
              } if (kp.Key == ConsoleKey.LeftArrow && !kp.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                decpos();
              } if (kp.Key == ConsoleKey.RightArrow && !kp.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                incpos();
              } if (kp.Key == ConsoleKey.LeftArrow && kp.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                decpos();
                if (Char.IsLetterOrDigit(currentchar())) {
                  while (currentpos() != 0 && Char.IsLetterOrDigit(currentchar())) decpos();
                  if (!Char.IsLetterOrDigit(currentchar())) incpos();
                } else {
                  while (currentpos() != 0 && !Char.IsLetterOrDigit(currentchar())) decpos();
                  if (Char.IsLetterOrDigit(currentchar())) {
                    while (currentpos() != 0 && Char.IsLetterOrDigit(currentchar())) decpos();
                    if (!Char.IsLetterOrDigit(currentchar())) incpos();
                  }
                }
              } if (kp.Key == ConsoleKey.RightArrow && kp.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                incpos();
                if (Char.IsLetterOrDigit(currentchar())) {
                  while (currentpos() != 0 && Char.IsLetterOrDigit(currentchar())) incpos();
                  if (!Char.IsLetterOrDigit(currentchar())) {
                    while (currentpos() != 0 && !Char.IsLetterOrDigit(currentchar())) incpos();
                  }
                } else {
                  while (currentpos() != 0 && !Char.IsLetterOrDigit(currentchar())) incpos();
                }
              } else if (kp.Key == ConsoleKey.UpArrow) {
                if (historypos > 0) {
                  gotopos(0);
                  dechistorypos();
                  buf = currenthistory();
                  gotopos(buf.Length);
                }
              } else if (kp.Key == ConsoleKey.DownArrow) {
                if (historypos < shortList.Count) {
                  gotopos(0);
                  inchistorypos();
                  buf = currenthistory();
                  gotopos(buf.Length);
                }
              } else if (kp.Key == ConsoleKey.Insert || (kp.Key == ConsoleKey.V && kp.Modifiers.HasFlag(ConsoleModifiers.Control))) {
                var text = Clipboard.GetText();
                var buf1 = new StringBuilder();
                if (currentpos() > 0) buf1.Append(buf.Substring(0, currentpos()));
                buf1.Append(text);
                if (currentpos() < buf.Length) buf1.Append(buf.Substring(currentpos(), buf.Length - currentpos()));
                buf = buf1.ToString();
                gotopos(currentpos() + text.Length);
              } else if (kp.Key == ConsoleKey.Escape) {
                gotopos(0);
                buf = "";
              } else if (kp.Key == ConsoleKey.Delete) {
                var buf1 = new StringBuilder();
                if (currentpos() > 0) buf1.Append(buf.Substring(0, currentpos()));
                if (currentpos() < buf.Length && buf.Length > 1) buf1.Append(buf.Substring(currentpos() + 1, buf.Length - currentpos() - 1));
                buf = buf1.ToString();
              } else if (kp.Key == ConsoleKey.Backspace) {
                if (currentpos() > 0) {
                  var buf1 = new StringBuilder();
                  if (currentpos() > 0 && buf.Length > 1) buf1.Append(buf.Substring(0, currentpos() - 1));
                  if (currentpos() < buf.Length) buf1.Append(buf.Substring(currentpos(), buf.Length - currentpos()));
                  buf = buf1.ToString();
                  decpos();
                }
              }
            }

            log(String.Format("[result] buf = {0}, currentpos = {1}", buf, currentpos()));
            redraw();
            log(String.Format("[finally] pos: x = {0}, y = {1}", System.Console.CursorLeft, System.Console.CursorTop));
          }

          return buf;
        }
      }))();

      if (input == String.Empty && !String.IsNullOrEmpty(prompt)) input = @default;
      if (input == "<empty>" && !String.IsNullOrEmpty(prompt)) input = String.Empty;

      if (Config.verbose) {
        println("all right, input is: {0}", input);
        println();
      }

      reg.SetValue("arguments" + (maxIndex + 1), input ?? "", RegistryValueKind.String);
      reg.SetValue("lastarguments", input ?? "", RegistryValueKind.String);
      return input;
    } else {
      if (prompt != null && prompt != String.Empty) print(prompt + ": ");
      return System.Console.ReadLine();
    }
  }

  private static EvalResult internalEval(String command, DirectoryInfo home = null) {
    var script = Path.GetTempFileName() + ".bat";
    File.AppendAllText(script, "@echo off" + "\r\n"); // always @echo off, so that eval works correctly
    File.AppendAllText(script, "cd /D \"" + (home ?? new DirectoryInfo(".")).FullName + "\"" + "\r\n");
    File.AppendAllText(script, command + "\r\n");
    File.AppendAllText(script, "exit /b %errorlevel%");

    var psi = new ProcessStartInfo();
    psi.FileName = script;
    psi.WorkingDirectory = (home ?? new DirectoryInfo(".")).FullName;
    psi.UseShellExecute = false;
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardError = true;

    if (Config.verbose) {
      Console.println("psi: filename = {0}, arguments = {1}, home = {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      Console.println("script at {0} is:", script);
      Console.println(File.ReadAllText(script));
      Console.println();
      Console.println("========================================");
      Console.println("The command will now be executed by cmd.exe");
      Console.println("========================================");
      Console.println();
    }

    if (Config.dryrun) {
      println("eval: {0} {1} at {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      return null;
    } else {
      var p = new Process();
      p.StartInfo = psi;

      var buf = new StringBuilder();
      p.OutputDataReceived += (sender, args) => { if (args.Data != null) buf.Append(args.Data + "\r\n"); };
      p.ErrorDataReceived += (sender, args) => { if (args.Data != null) buf.Append(args.Data + "\r\n  "); };

      if (p.Start()) {
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
        return new EvalResult(p.ExitCode, buf.ToString().Split(new []{"\r\n"}, StringSplitOptions.None).ToList());
      } else {
        return null;
      }
    }
  }

  [DllImport("Kernel32")]
  public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

  public delegate bool HandlerRoutine(CtrlTypes CtrlType);

  public enum CtrlTypes{
      CTRL_C_EVENT = 0,
      CTRL_BREAK_EVENT,
      CTRL_CLOSE_EVENT,
      CTRL_LOGOFF_EVENT = 5,
      CTRL_SHUTDOWN_EVENT
  }

  private static ExitCode cmd(String command, DirectoryInfo home = null, bool trace = false) {
    //var script = Path.GetTempFileName() + ".bat";
    //if (!Config.verbose) File.AppendAllText(script, "@echo off" + "\r\n");
    //File.AppendAllText(script, "cd /D \"" + (home ?? new DirectoryInfo(".")).FullName + "\"" + "\r\n");
    //File.AppendAllText(script, command + "\r\n");
    //File.AppendAllText(script, "exit /b %errorlevel%");

    var psi = new ProcessStartInfo();
    //psi.FileName = script;
    psi.FileName = "cmd.exe";
    psi.Arguments = "/C " + command;
    psi.WorkingDirectory = (home ?? new DirectoryInfo(".")).FullName;
    psi.UseShellExecute = false;

    if (Config.verbose) {
      Console.println("psi: filename = {0}, arguments = {1}, home = {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      //Console.println("script at {0} is:", script);
      //Console.println(File.ReadAllText(script));
      Console.println();
      Console.println("========================================");
      Console.println("The command will now be executed by cmd.exe");
      Console.println("========================================");
      Console.println();
    }

    if (Config.dryrun) {
      println("exec: {0} {1} at {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      return -1;
    } else {
      var p = new Process();
      p.StartInfo = psi;

      traceln("psi: filename = {0}, arguments = {1}, home = {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      if (trace) {
        var lck = new Object();
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        p.OutputDataReceived += (sender, args) => { lock(lck) { if (args.Data != null) println(args.Data); }; };
        p.ErrorDataReceived += (sender, args) => { lock(lck) { if (args.Data != null) println(args.Data); }; };
      } else {
        traceln("<interactive session, not traced>");
      }

      if (p.Start()) {
        //SetConsoleCtrlHandler(ctype => { System.Console.WriteLine("received CTRL_EVENT: " + ctype); return true; }, true);
        if (trace) { p.BeginOutputReadLine(); p.BeginErrorReadLine(); }
        p.WaitForExit();
        return p.ExitCode;
      } else {
        return -1;
      }
    }
  }

  private static ExitCode shellex(String command, DirectoryInfo home = null) {
    command = command.TrimStart(new []{' '});

    var i = 0;
    var buf = new StringBuilder();
    var in_quote = false;
    Func<bool> prev_slash = () => i == 1 ? false : command[i - 2] == '\\';
    while (i < command.Length) {
      var c = command[i++];

      if (c == '\"') {
        if (prev_slash()) {
          buf.Remove(buf.Length - 1, 1);
          buf.Append(c);
        } else {
          if (in_quote) {
            in_quote = false;
            break;
          } else {
            in_quote = true;
          }
        }
      } else if (c == '\\') {
        if (prev_slash()) {
          buf.Remove(buf.Length - 1, 1);
          buf.Append(c);
        } else {
          buf.Append(c);
        }
      } else if (c == ' ') {
        if (prev_slash()) {
          buf.Remove(buf.Length - 1, 1);
          buf.Append(c);
        } else {
          if (in_quote) {
            buf.Append(c);
          } else {
            break;
          }
        }
      } else {
        buf.Append(c);
      }
    }

    var psi = new ProcessStartInfo();
    psi.FileName = buf.ToString();
    psi.Arguments = command.Substring(i);
    psi.WorkingDirectory = (home ?? new DirectoryInfo(".")).FullName;
    psi.UseShellExecute = true;

    if (Config.verbose) {
      Console.println("psi: filename = {0}, arguments = {1}, home = {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      Console.println();
      Console.println("========================================");
      Console.println("The command will now be executed by shellex");
      Console.println("========================================");
      Console.println();
    }

    if (Config.dryrun) {
      println("shellexec: {0} {1} at {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      return -1;
    } else {
      var p = new Process();
      p.StartInfo = psi;

      if (!Console.firstTrace) {
        traceln("psi: filename = {0}, arguments = {1}, home = {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
        traceln("<ui session, not traced>");
      }

      p.Start();
      return 0;
    }
  }

  public class EvalResult {
    public ExitCode status;
    public List<String> lines;

    public EvalResult(ExitCode status, List<String> lines) {
      this.status = status;
      this.lines = lines;
    }

    public static EvalResult operator !(EvalResult c) {
      return new EvalResult(!c.status, c.lines);
    }

    public static EvalResult operator &(EvalResult c1, EvalResult c2) {
      return c1 ? c2 : c1;
    }

    public static EvalResult operator |(EvalResult c1, EvalResult c2) {
      return c1 ? c1 : c2;
    }

    public static bool operator true(EvalResult c) {
      return (bool)c.status;
    }

    public static bool operator false(EvalResult c) {
      return (bool)c.status;
    }

    public static implicit operator bool(EvalResult c) {
      return (bool)c.status;
    }
  }

  public static EvalResult eval(String command) {
    return eval(command, null as DirectoryInfo);
  }

  public static EvalResult eval(String command, String home = null) {
    return eval(command, home == null ? null as DirectoryInfo : new DirectoryInfo(home));
  }

  public static EvalResult eval(String command, DirectoryInfo home = null) {
    if (home == null) {
      home = new DirectoryInfo(".");
      if (Directory.Exists(Config.target)) home = new DirectoryInfo(Config.target);
      if (File.Exists(Config.target)) home = new FileInfo(Config.target).Directory;
    }

    Config.env["workingDir"] = home.FullName;
//    System.Console.WriteLine("eval " + command + ": " + Config.env["workingDir"]);
//    System.Console.WriteLine(new StackTrace(true).ToString());
    return internalEval(command, home);
  }

  public static ExitCode batch(String command) {
    return batch(command, null as DirectoryInfo);
  }

  public static ExitCode batch(String command, String home = null) {
    return batch(command, home == null ? null as DirectoryInfo : new DirectoryInfo(home));
  }

  public static ExitCode batch(String command, DirectoryInfo home = null) {
    if (home == null) {
      home = new DirectoryInfo(".");
      if (Directory.Exists(Config.target)) home = new DirectoryInfo(Config.target);
      if (File.Exists(Config.target)) home = new FileInfo(Config.target).Directory;
    }

    Config.env["workingDir"] = home.FullName;
//    System.Console.WriteLine("batch " + command + ": " + Config.env["workingDir"]);
//    System.Console.WriteLine(new StackTrace(true).ToString());
    return cmd(command, home, true);
  }

  public static ExitCode interactive(String command) {
    return interactive(command, null as DirectoryInfo);
  }

  public static ExitCode interactive(String command, String home = null) {
    return interactive(command, home == null ? null as DirectoryInfo : new DirectoryInfo(home));
  }

  public static ExitCode interactive(String command, DirectoryInfo home = null) {
    if (home == null) {
      home = new DirectoryInfo(".");
      if (Directory.Exists(Config.target)) home = new DirectoryInfo(Config.target);
      if (File.Exists(Config.target)) home = new FileInfo(Config.target).Directory;
    }

    Config.env["workingDir"] = home.FullName;
//    System.Console.WriteLine("interactive " + command + ": " + Config.env["workingDir"]);
//    System.Console.WriteLine(new StackTrace(true).ToString());
    return cmd(command, home, false);
  }

  public static ExitCode ui(String command) {
    return ui(command, null as DirectoryInfo);
  }

  public static ExitCode ui(String command, String home = null) {
    return ui(command, home == null ? null as DirectoryInfo : new DirectoryInfo(home));
  }

  public static ExitCode ui(String command, DirectoryInfo home = null) {
    if (home == null) {
      home = new DirectoryInfo(".");
      if (Directory.Exists(Config.target)) home = new DirectoryInfo(Config.target);
      if (File.Exists(Config.target)) home = new FileInfo(Config.target).Directory;
    }

    Config.env["workingDir"] = home.FullName;
//    System.Console.WriteLine("ui " + command + ": " + Config.env["workingDir"]);
//    System.Console.WriteLine(new StackTrace(true).ToString());
    return shellex(command, home);
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}

public static class Shell {
  public static String ShellEscape(this String s) {
    return s.Contains(" ") ? "\"" + s + "\"" : s;
  }
}

public static class FileSystem {
  public static bool IsEquivalentTo(this FileSystemInfo fsi1, FileSystemInfo fsi2) {
    if (fsi1 == null || fsi2 == null) return fsi1 == null && fsi2 == null;
    return fsi1.GetRealPath().FullName.ToUpper() == fsi2.GetRealPath().FullName.ToUpper();
  }

  public static bool IsEquivalentTo(this FileSystemInfo fsi1, String fsi2) {
    if (fsi1 == null || fsi2 == null) return fsi1 == null && fsi2 == null;
    return fsi1.GetRealPath().FullName.ToUpper() == fsi2.GetRealPath().ToUpper();
  }

  public static bool IsEquivalentTo(this String fsi1, FileSystemInfo fsi2) {
    if (fsi1 == null || fsi2 == null) return fsi1 == null && fsi2 == null;
    return fsi1.GetRealPath().ToUpper() == fsi2.FullName.GetRealPath().ToUpper();
  }

  public static bool IsEquivalentTo(this String fsi1, String fsi2) {
    if (fsi1 == null || fsi2 == null) return fsi1 == null && fsi2 == null;
    return fsi1.GetRealPath().ToUpper() == fsi2.GetRealPath().ToUpper();
  }

  public static bool IsParentOf(this FileSystemInfo fsi1, FileSystemInfo fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.GetRealPath().FullName.ToUpper().Replace("/", "\\").StartsWith(fsi1.GetRealPath().FullName.ToUpper().Replace("/", "\\") + "\\");
  }

  public static bool IsParentOf(this FileSystemInfo fsi1, String fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.GetRealPath().ToUpper().Replace("/", "\\").StartsWith(fsi1.GetRealPath().FullName.ToUpper().Replace("/", "\\") + "\\");
  }

  public static bool IsParentOf(this String fsi1, FileSystemInfo fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.GetRealPath().FullName.ToUpper().Replace("/", "\\").StartsWith(fsi1.GetRealPath().ToUpper().Replace("/", "\\") + "\\");
  }

  public static bool IsParentOf(this String fsi1, String fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.GetRealPath().ToUpper().Replace("/", "\\").StartsWith(fsi1.GetRealPath().ToUpper().Replace("/", "\\") + "\\");
  }

  public static bool IsParentOrEquivalentTo(this FileSystemInfo fsi1, FileSystemInfo fsi2) {
    return fsi1.IsParentOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsParentOrEquivalentTo(this FileSystemInfo fsi1, String fsi2) {
    return fsi1.IsParentOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsParentOrEquivalentTo(this String fsi1, FileSystemInfo fsi2) {
    return fsi1.IsParentOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsParentOrEquivalentTo(this String fsi1, String fsi2) {
    return fsi1.IsParentOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsChildOf(this FileSystemInfo fsi1, FileSystemInfo fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.IsParentOf(fsi1);
  }

  public static bool IsChildOf(this FileSystemInfo fsi1, String fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.IsParentOf(fsi1);
  }

  public static bool IsChildOf(this String fsi1, FileSystemInfo fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.IsParentOf(fsi1);
  }

  public static bool IsChildOf(this String fsi1, String fsi2) {
    if (fsi1 == null || fsi2 == null) return false;
    return fsi2.IsParentOf(fsi1);
  }

  public static bool IsChildOrEquivalentTo(this FileSystemInfo fsi1, FileSystemInfo fsi2) {
    return fsi1.IsChildOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsChildOrEquivalentTo(this FileSystemInfo fsi1, String fsi2) {
    return fsi1.IsChildOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsChildOrEquivalentTo(this String fsi1, FileSystemInfo fsi2) {
    return fsi1.IsChildOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  public static bool IsChildOrEquivalentTo(this String fsi1, String fsi2) {
    return fsi1.IsChildOf(fsi2) || fsi1.IsEquivalentTo(fsi2);
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
  private static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] String path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

  public static String GetShortPath(this String path) {
    var buf = new StringBuilder(255);
    var status = GetShortPathName(path, buf, buf.Capacity);
    if (status == 0) return null;
    return buf.ToString();
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
  private static extern int GetLongPathName([MarshalAs(UnmanagedType.LPTStr)] String path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

  public static String GetLongPath(this String path) {
    var buf = new StringBuilder(255);
    var status = GetLongPathName(path, buf, buf.Capacity);
    if (status == 0) return null;
    return buf.ToString();
  }

  public static bool IsSymlink(this FileSystemInfo fsi) {
    if (fsi == null) return false;
    if (fsi is FileInfo) return ((FileInfo)fsi).IsSymlink();
    if (fsi is DirectoryInfo) return ((DirectoryInfo)fsi).IsSymlink();
    throw new NotSupportedException(String.Format("File system object {0} is not supported", fsi.GetType()));
  }

  public static bool IsSymlink(this FileInfo file) {
    if (file == null) return false;
    return file.Attributes.HasFlag(FileAttributes.ReparsePoint);
  }

  public static bool IsSymlink(this DirectoryInfo dir) {
    if (dir == null) return false;
    return dir.Attributes.HasFlag(FileAttributes.ReparsePoint);
  }

  public static bool IsSymlink(this String path) {
    if (path == null) return false;
    var file = new FileInfo(path);
    if (file.Exists) return file.IsSymlink();

    var dir = new DirectoryInfo(path);
    if (dir.Exists) return dir.IsSymlink();

    return false;
  }

  // copy/pasted from http://chrisbensen.blogspot.com/2010/06/getfinalpathnamebyhandle.html
  private const int FILE_SHARE_READ = 1;
  private const int FILE_SHARE_WRITE = 2;
  private const int CREATION_DISPOSITION_OPEN_EXISTING = 3;
  private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

  // http://msdn.microsoft.com/en-us/library/aa364962%28VS.85%29.aspx
  [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern int GetFinalPathNameByHandle(IntPtr handle, [In, Out] StringBuilder path, int bufLen, int flags);

  // http://msdn.microsoft.com/en-us/library/aa363858(VS.85).aspx
  [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr SecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

  public static FileSystemInfo GetRealPath(this FileSystemInfo fsi) {
    if (fsi == null) return null;
    if (fsi is FileInfo) return ((FileInfo)fsi).GetRealPath();
    if (fsi is DirectoryInfo) return ((DirectoryInfo)fsi).GetRealPath();
    throw new NotSupportedException(String.Format("File system object {0} is not supported", fsi.GetType()));
  }

  public static FileInfo GetRealPath(this FileInfo file) {
    if (file == null) return null;
    return new FileInfo(file.FullName.GetRealPath());
  }

  public static DirectoryInfo GetRealPath(this DirectoryInfo dir) {
    if (dir == null) return null;
    return new DirectoryInfo(dir.FullName.GetRealPath());
  }

  public static String GetRealPath(this String path) {
    if (path == null) return null;
    path = Path.GetFullPath(path);
    if (!File.Exists(path) && !Directory.Exists(path)) return path;

    for (var parent = Path.GetDirectoryName(path); parent != null; parent = Path.GetDirectoryName(parent)) {
      if (parent.IsSymlink()) {
        var realParent = parent.GetRealPath();
        if (!realParent.EndsWith("\\")) realParent += "\\";
        if (!parent.EndsWith("\\")) parent += "\\";
        var relPath = path.Substring(parent.Length);
        return (realParent + relPath).GetRealPath();
      }
    }

    var file = new FileInfo(path);
    if (file.Exists && !file.Attributes.HasFlag(FileAttributes.ReparsePoint)) return path;
    var dir = new DirectoryInfo(path);
    if (dir.Exists && !dir.Attributes.HasFlag(FileAttributes.ReparsePoint)) return path;

    var directoryHandle = CreateFile(path, 0, 2, IntPtr.Zero, CREATION_DISPOSITION_OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
    if (directoryHandle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());

    var buf = new StringBuilder(512);
    int size = GetFinalPathNameByHandle(directoryHandle.DangerousGetHandle(), buf, buf.Capacity, 0);
    if (size < 0) throw new Win32Exception(Marshal.GetLastWin32Error());

    // The remarks section of GetFinalPathNameByHandle mentions the return being prefixed with "\\?\"
    // More information about "\\?\" here -> http://msdn.microsoft.com/en-us/library/aa365247(v=VS.85).aspx
    if (buf[0] == '\\' && buf[1] == '\\' && buf[2] == '?' && buf[3] == '\\')
    return buf.ToString().Substring(4);
    else return buf.ToString();
  }
}

public static class Config {
  public static bool sublime;
  public static bool dryrun;
  public static bool verbose;
  public static String action;
  public static String originalTarget;
  public static String target;
  public static String rawTarget;
  public static String sanitizedRawTarget { get {
    var iof = rawTarget.IndexOf(" ");
    if (iof == -1) return rawTarget;
    else return rawTarget.Substring(0, iof - 1);
  } }
  public static String rawCommandLine;
  public static String requestedConn;
  public static Arguments args;
  public static Conn conn;
  public static MethodInfo conn_action;
  public static Dictionary<String, String> env;

  public static ExitCode parse(String[] args) {
    Config.env = new Dictionary<String, String>();
    env["workingDir"] = Environment.CurrentDirectory;
//    System.Console.WriteLine("initial: " + Config.env["workingDir"]);
    Config.env["traceFile"] = Trace.auxiliaryFileName("NA", "NA");

    Func<ExitCode> extractSystemFlags = () => {
      var systemFlags = args.TakeWhile(arg => arg.StartsWith("/")).Select(flag => flag.ToUpper()).ToList();
      args = args.SkipWhile(arg => arg.StartsWith("/")).ToArray();
      if (systemFlags.Where(flag => flag == "/?").Count() > 0) { Help.printUsage(); return -1; }
      if (args.ElementAtOrDefault(0) == "-help" || args.ElementAtOrDefault(0) == "--help") { Help.printUsage(); return -1; }
      Config.sublime = Config.sublime || systemFlags.Contains("/S");
      Config.dryrun = Config.dryrun || systemFlags.Contains("/D");
      Config.verbose = Config.verbose || systemFlags.Contains("/V");
      var f_requestedConn = systemFlags.LastOrDefault(flag => flag.StartsWith("/C:"));
      if (f_requestedConn != null) Config.requestedConn = f_requestedConn.Substring(3);
      return 0;
    };

    args = args.Select(arg => arg.Trim()).Where(arg => arg != "").ToArray();

    if (!extractSystemFlags()) return -1;
    var action = args.ElementAtOrDefault(0);
    args = args.Skip(1).ToArray();
    if (action == null) { Help.printUsage(); return -1; }
    Config.action = action;
    env["action"] = action;
    Config.env["traceFile"] = Trace.auxiliaryFileName("NA", Config.action);

    if (!extractSystemFlags()) return -1;
    // var flags = args.TakeWhile(arg => arg.StartsWith("-")).ToList();
    // args = args.SkipWhile(arg => arg.StartsWith("-")).ToArray();
    var flags = new List<String>();
    var specialFlags = new []{"-cp", "-bootcp", "-toolcp", "-d"};
    var i = 0;
    while (i < args.Length) {
      var arg = args[i];
      if (arg.StartsWith("-")) {
        flags.Add(arg);
        i += 1;
        if (specialFlags.Contains(arg) && i < args.Length) {
          arg = args[i];
          flags.Add(arg);
          i += 1;
        }
      } else {
        break;
      }
    }
    args = args.Skip(i).ToArray();

    var rawTarget = args.Take(1).ElementAtOrDefault(0) ?? "";
    Config.rawTarget = rawTarget;
    Config.rawCommandLine = String.Join(" ", flags.ToArray()) + " " + String.Join(" ", args.ToArray());

    var target = args.Take(1).ElementAtOrDefault(0) ?? ".";
    if (target.Contains("*") || target.Contains("?")) {
      target = ".";
    } else {
      args = args.Skip(1).ToArray();
    }
    if (target == null || target == "/?" || target == "-help" || target == "--help") { Help.printUsage(action); return -1; }
    env["target"] = target;

    // don't check for file existence - connector might actually create that non-existent file/directory
    try {
      Config.originalTarget = Path.GetFullPath(target);
      Config.target = Path.GetFullPath(target);
    } catch {
      Config.originalTarget = target;
      Config.target = target;
    }
    args = args.Where(arg => arg.Trim() != String.Empty).ToArray();
    Config.args = new Arguments(Enumerable.Concat(flags, args).ToList());
    env["args"] = String.Join(" ", Config.args.ToArray());

    // conn will be set elsewhere
    Config.conn = null;

    return 0;
  }

  public static void print() {
    Console.println("config: dryrun = {0}, verbose = {1}, action = {2}, target = {3}, args = {4}", dryrun, verbose, action, target, String.Join(", ", args));
  }
}

public static class Trace {
  public static String primaryFileName(String conn_name, String action) {
    var traceDir = new DirectoryInfo(@"%HOME%\.myke_important".Expand());
    if (!traceDir.Exists) traceDir.Create();
    return traceDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + conn_name + "-" + action + ".log";
  }

  public static String primaryFileName(String file_name) {
    var traceDir = new DirectoryInfo(@"%HOME%\.myke_important".Expand());
    if (!traceDir.Exists) traceDir.Create();
    var liof = file_name.Replace("/", "\\").LastIndexOf("\\");
    return traceDir + "\\" + file_name.Substring(liof);
  }

  public static String auxiliaryFileName(String conn_name, String action) {
    var traceDir = new DirectoryInfo(@"%HOME%\.myke_all".Expand());
    if (!traceDir.Exists) traceDir.Create();
    return traceDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + conn_name + "-" + action + ".log";
  }

  public static String auxiliaryFileName(String file_name) {
    var traceDir = new DirectoryInfo(@"%HOME%\.myke_all".Expand());
    if (!traceDir.Exists) traceDir.Create();
    var liof = file_name.Replace("/", "\\").LastIndexOf("\\");
    return traceDir + "\\" + file_name.Substring(liof);
  }
}

public static class Help {
  public static void printUsage(String action = null) {
    if (action == null) {
      Console.println();
      Console.println("myke [/D] [/V] [action] [target] [args...]");
      Console.println("  action :  one of \"compile\", \"rebuild\", \"run\", \"repl\", \"test\"; defaults to \"compile\"");
      Console.println("            also supports \"commit\", \"logall\", \"logthis\", \"log\", \"push\", \"pull\"");
      Console.println("  target :  a single file or directory name that will hint what to do next; defaults to \".\"");
      Console.println("            you cannot provide multiple input file/directory names (e.g. as in scalac)");
      Console.println("            this is by design to be bijectively compatible with single-file editors");
      Console.println("            if you do need this, consider configuring your project with some build system");
      Console.println("            and hand-editing myke.cs to integrate with it (e.g. this is how it was done for sbt)");
      Console.println("  args   :  custom data that will be passed to the handler of the command; defaults to \"\"");
      Console.println();
      Console.println("flags:");
      Console.println("  /D     :  dry run, only resolve the connector, but don't do anything; disabled by default");
      Console.println("  /V     :  enables verbose mode of execution; disabled by default");
      Console.println();
      Console.println("see https://raw.github.com/xeno-by/dotwindows/master/Scripts/Scripts for more information");
      Console.println("myke.cs represents the core, while myke-*.cs implement connectors to various build engines");
      Console.println();
      Console.println("connectors:");
    } else {
      Console.println();
      Console.println("action \"{0}\" is supported by the following connectors:", Config.action);
      Console.println();
    }

    printConnectors(action);
  }

  public static void printConnectors(String action = null) {
    var connectors = Connectors.all.Where(connector => action == null || connector.actions().ContainsKey(action)).ToList();
    var maxlen = connectors.Max(connector => connector.name().Length);

    for (var i = 0; i < connectors.Count; ++i) {
      var connector = connectors[i];
      Console.println("* {0} (supports {1})", connector.name(), String.Join(", ",
        connector.actions().Select(kvp => kvp.Key != "default" ? kvp.Key : String.Format("{0} = {1}", kvp.Key, kvp.Value.name()))));
      Console.println(connector.description());
    }
  }
}

public class ConnectorAttribute : Attribute {
  public String name;
  public String description;
  public double priority;
}

public class ActionAttribute : Attribute {
  public String name;

  public ActionAttribute() {
  }

  public ActionAttribute(String name) {
    this.name = name;
  }
}

public class MenuItemAttribute : Attribute {
  public String hotkey;
  public String description;
  public double priority;
}

public class DefaultAttribute : Attribute {
}

public class DontTraceAttribute : Attribute {
}

public class MeaningfulAttribute : Attribute {
}

public static class Connectors {
  public class ConnectorsComparer : IComparer<Object> {
    public int Compare(Object x, Object y) {
      if (x == null || y == null) return x == null && y == null ? 0 : x == null ? -1 : 1;
      if (!(x is Type) || !(y is Type)) return Compare(x.GetType(), y.GetType());
      return Compare((Type)x, (Type)y);
    }

    public int Compare(Type x, Type y) {
      if (x.priority() > y.priority()) return -1;
      if (x.priority() < y.priority()) return 1;

      for (var x0 = x.BaseType; x0 != null; x0 = x0.BaseType) if (x0 == y) return -1;
      for (var y0 = y.BaseType; y0 != null; y0 = y0.BaseType) if (y0 == x) return 1;
      return 0;
    }
  }

  public static List<Type> all { get {
    var root = Assembly.GetExecutingAssembly();
    var t_connectors = root.GetTypes().Where(t => t.IsDefined(typeof(ConnectorAttribute), true) && !t.IsInterface && !t.IsAbstract).ToList();
    t_connectors.Sort(new ConnectorsComparer());
    return t_connectors;
  } }

  public static String name(this Object connector) {
    if (connector is Type) return ((Type)connector).name();
    if (connector is MethodInfo) return ((MethodInfo)connector).name();
    return connector.GetType().name();
  }

  public static String name(this Type connector) {
    var attr = connector.GetCustomAttributes(typeof(ConnectorAttribute), true).Cast<ConnectorAttribute>().Single();
    return attr.name;
  }

  public static String name(this MethodInfo meth) {
    var attr = meth.GetCustomAttributes(typeof(ActionAttribute), true).Cast<ActionAttribute>().Single();
    if (attr.name != null) return attr.name;

    var name = meth.Name;
    var buf = new StringBuilder();
    for (var i = 0; i < name.Length; ++i) {
      var prev = i == 0 ? '\0' : name[i - 1];
      var curr = name[i];
      var next = i == name.Length - 1 ? '\0' : name[i + 1];
      if (Char.IsLower(prev) && Char.IsUpper(curr) && !Char.IsUpper(next)) {
        buf.Append("-");
        buf.Append(Char.ToLower(curr));
      } else {
        buf.Append(curr);
      }
    }

    return buf.ToString();
  }

  public static String description(this Object connector) {
    if (connector is Type) return ((Type)connector).description();
    if (connector is MethodInfo) return ((MethodInfo)connector).description();
    return connector.GetType().description();
  }

  public static String description(this Type connector) {
    var attr = connector.GetCustomAttributes(typeof(ConnectorAttribute), true).Cast<ConnectorAttribute>().Single();
    return attr.description;
  }

  public static String description(this MethodInfo connector) {
    var attr = connector.GetCustomAttributes(typeof(MenuItemAttribute), true).Cast<MenuItemAttribute>().Single();
    return attr.description;
  }

  public static String hotkey(this Object connector) {
    if (connector is MethodInfo) return ((MethodInfo)connector).description();
    throw new Exception(connector == null ? "null" : connector.ToString());
  }

  public static String hotkey(this MethodInfo connector) {
    var attr = connector.GetCustomAttributes(typeof(MenuItemAttribute), true).Cast<MenuItemAttribute>().SingleOrDefault();
    return attr == null ? null : attr.hotkey;
  }

  public static double priority(this Object connector) {
    if (connector is Type) return ((Type)connector).priority();
    if (connector is MethodInfo) return ((MethodInfo)connector).priority();
    return connector.GetType().priority();
  }

  public static double priority(this Type connector) {
    var attr = connector.GetCustomAttributes(typeof(ConnectorAttribute), true).Cast<ConnectorAttribute>().Single();
    return attr.priority;
  }

  public static double priority(this MethodInfo meth) {
    var attr = meth.GetCustomAttributes(typeof(MenuItemAttribute), true).Cast<MenuItemAttribute>().Single();
    return attr.priority;
  }

  public static Dictionary<String, MethodInfo> actions(this Type connector) {
    var methods = connector.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.IsDefined(typeof(ActionAttribute), true)).ToList();
    var map = methods.ToDictionary(meth => meth.name(), meth => meth);

    methods.ForEach(meth => {
      var attr = meth.GetCustomAttributes(typeof(DefaultAttribute), true).Cast<DefaultAttribute>().SingleOrDefault();
      if (attr != null) map.Add("default", meth);
    });

    return map;
  }

  public static Dictionary<String, Func<ExitCode>> actions(this Object connector) {
    var t_actions = connector.GetType().actions();
    return t_actions.Keys.ToDictionary<String, String, Func<ExitCode>>(key => key, key => {
      var method = t_actions[key];
      var args = method.bindArgs();
      return () => (ExitCode)method.Invoke(connector, args);
    });
  }

  public class MenuItemsComparer : IComparer<Object> {
    public int Compare(Object x, Object y) {
      if (x == null || y == null) return x == null && y == null ? 0 : x == null ? -1 : 1;
      if (!(x is MethodInfo) || !(y is MethodInfo)) throw new Exception(String.Format("x is {0}, y is {1}", x == null ? "null" : x.GetType().ToString(), y == null ? "null" : y.GetType().ToString()));
      return Compare((MethodInfo)x, (MethodInfo)y);
    }

    public int Compare(MethodInfo x, MethodInfo y) {
      if (x.priority() > y.priority()) return -1;
      if (x.priority() < y.priority()) return 1;
      return 0;
    }
  }

  public static List<String> menuitems(this Object connector) {
    var methods = connector.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.IsDefined(typeof(MenuItemAttribute), true)).ToList();
    methods.Sort(new MenuItemsComparer());
    var i = 1;
    return methods.Select(meth => {
      var hotkey = meth.hotkey() ?? (i++).ToString();
      return String.Format("{0}. {1}", hotkey, meth.description());
    }).ToList();
  }

  public static Func<ExitCode> menuitem(this Object connector, String hotkey) {
    var methods = connector.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.IsDefined(typeof(MenuItemAttribute), true)).ToList();
    methods.Sort(new MenuItemsComparer());
    var hotkeys = new Dictionary<String, MethodInfo>();
    var i = 1;
    methods.ForEach(meth => {
      var hotkey1 = meth.hotkey() ?? (i++).ToString();
      hotkeys.Add(hotkey1, meth);
    });
    if (hotkeys.ContainsKey(hotkey)) {
      var method = hotkeys[hotkey];
      return (Func<ExitCode>)(() => {
        var args = method.bindArgs();
        return (ExitCode)method.Invoke(connector, args);
      });
    } else {
      return null;
    }
  }

  public static Boolean meaningful(this Object connector) {
    if (connector is Type) return ((Type)connector).meaningful();
    if (connector is MethodInfo) return ((MethodInfo)connector).meaningful();
    return connector.GetType().meaningful();
  }

  public static Boolean meaningful(this Type connector) {
    return connector.GetCustomAttributes(typeof(MeaningfulAttribute), true).Cast<MeaningfulAttribute>().Count() > 0;
  }

  public static Boolean meaningful(this MethodInfo connector) {
    return connector.GetCustomAttributes(typeof(MeaningfulAttribute), true).Cast<MeaningfulAttribute>().Count() > 0;
  }

  public static Object instantiate(this Type connector) {
    var ctors = connector.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(ctor1 => ctor1.GetParameters().Where(p => !p.IsOptional).Count() > 0).ToList();
    var binds = ctors.ToDictionary(ctor1 => ctor1, ctor1 => ctor1.bindArgs());
    var bind = binds.Where(kvp => kvp.Value != null).SingleOrDefault();
    var ctor = bind.Key;
    var args = bind.Value;
    return args == null ? null : ctor.Invoke(args);
  }

  public static bool accept(this Object connector) {
    var method = connector.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "accept").Single();
    var args = method.bindArgs();
    return args == null ? false : (bool)method.Invoke(connector, args);
  }

  public static ExitCode action(this Object connector, String action) {
    return connector.actions()[action]();
  }

  public static bool canTrace(this MethodInfo action) {
    if (action == null) return false;
    var attrs = action.GetCustomAttributes(typeof(DontTraceAttribute), true).Cast<DontTraceAttribute>().ToList();
    return attrs.Count() == 0;
  }

  private static Object[] bindArgs(this MethodBase method) {
    try {
      return method.GetParameters().Select<ParameterInfo, Object>(p => {
        if (p.ParameterType == typeof(FileInfo)) {
          var file = new FileInfo(Config.target);
          if (!file.Exists) throw new FileNotFoundException(Config.target);
          return file;
        } else if (p.ParameterType == typeof(DirectoryInfo)) {
          var dir = new DirectoryInfo(Config.target);
          if (!dir.Exists) throw new DirectoryNotFoundException(Config.target);
          return dir;
        } else if (p.ParameterType == typeof(Lines)) {
          var file = new FileInfo(Config.target);
          if (!file.Exists) throw new FileNotFoundException(Config.target);
          return new Lines(file, File.ReadAllLines(file.FullName).ToList());
        } else if (p.ParameterType == typeof(Arguments)) {
          return Config.args;
        } else {
          if (p.IsOptional) return p.DefaultValue;
          throw new NotSupportedException(String.Format("cannot bind parameter {0} of type {1}", p.Name, p.ParameterType.FullName));
        }
      }).ToArray();
    } catch (Exception) {
      return null;
    }
  }
}

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy("System.Collections.Generic.Mscorlib_CollectionDebugView`1,mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089")]
[DebuggerNonUserCode]
public abstract class BaseList<T> : IList<T> {
  protected abstract IEnumerable<T> Read();

  public virtual bool IsReadOnly { get { return true; } }
  protected virtual void InsertAt(int index, T el) {}
  protected virtual void UpdateAt(int index, T el) {}
  public virtual void RemoveAt(int index) {}

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
  public IEnumerator<T> GetEnumerator() {
    return Read().GetEnumerator();
  }

  public void Add(T item) {
    if (IsReadOnly) throw new NotSupportedException("cannot add to a readonly list");
    Insert(Count, item);
  }

  public void Clear() {
    if (IsReadOnly) throw new NotSupportedException("cannot clear a readonly list");
    for (var i = 0; i < Count; ++i) RemoveAt(0);
  }

  public bool Contains(T item) {
    return IndexOf(item) != -1;
  }

  void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
    Read().ToArray().CopyTo(array, arrayIndex);
  }

  public bool Remove(T item) {
    if (IsReadOnly) throw new NotSupportedException("cannot remove from a readonly list");

    var index = IndexOf(item);
    if (index == -1) {
      return false;
    } else {
      RemoveAt(index);
      return true;
    }
  }

  public int Count {
    get { return Read().Count(); }
  }

  public int IndexOf(T item) {
    return Read().ToList().IndexOf(item);
  }

  public void Insert(int index, T item) {
    if (IsReadOnly) throw new NotSupportedException("cannot insert into a readonly list");
    InsertAt(index, item);
  }

  public T this[int index] {
    get { return Read().ElementAt(index); }
    set {
      if (IsReadOnly) throw new NotSupportedException("cannot update a readonly list");
      UpdateAt(index, value);
    }
  }
}

public class Lines : BaseList<String> {
  public FileInfo file;
  private List<String> lines;

  public Lines(FileInfo file, List<String> lines) {
    this.file = file;
    this.lines = lines;
  }

  protected override IEnumerable<String> Read() {
    return lines;
  }
}

public class Arguments : BaseList<String> {
  private List<String> arguments;

  public Arguments(List<String> arguments = null) {
    this.arguments = arguments ?? new List<String>();
  }

  protected override IEnumerable<String> Read() {
    return arguments;
  }

  public override String ToString() {
    return String.Join(" ", arguments.Select(arg => arg.ShellEscape()));
  }
}

public abstract class Base {
  protected static ExitCode print(String format, params Object[] objs) {
    Console.print(format, objs);
    return 0;
  }

  protected static ExitCode print(String obj) {
    Console.print(obj);
    return 0;
  }

  protected static ExitCode print(Object obj) {
    Console.print(obj);
    return 0;
  }

  protected static ExitCode println(String format, params Object[] objs) {
    Console.println(format, objs);
    return 0;
  }

  protected static ExitCode println(String obj) {
    Console.println(obj);
    return 0;
  }

  protected static ExitCode println(Object obj) {
    Console.println(obj);
    return 0;
  }

  protected static ExitCode println() {
    Console.println();
    return 0;
  }

  protected static ExitCode trace(String format, params Object[] objs) {
    Console.trace(format, objs);
    return 0;
  }

  protected static ExitCode trace(String obj) {
    Console.trace(obj);
    return 0;
  }

  protected static ExitCode trace(Object obj) {
    Console.trace(obj);
    return 0;
  }

  protected static ExitCode traceln(String format, params Object[] objs) {
    Console.traceln(format, objs);
    return 0;
  }

  protected static ExitCode traceln(String obj) {
    Console.traceln(obj);
    return 0;
  }

  protected static ExitCode traceln(Object obj) {
    Console.traceln(obj);
    return 0;
  }

  protected static ExitCode traceln() {
    Console.traceln();
    return 0;
  }

  protected static String readln(String prompt = null, String history = null) {
    return Console.readln(prompt, history);
  }

  protected virtual String transplantFrom { get { return null; } }

  protected virtual String transplantTo { get { return null; } }

  protected ExitCode transplantFile(String from, String to, Boolean overwrite = true) {
    from = (from.Expand().Contains(":") ? from : ((transplantFrom == null ? null : (transplantFrom + "\\")) + from)).Expand().Replace("/", "\\");
    to = (to.Expand().Contains(":") ? to : ((transplantTo == null ? null : (transplantTo + "\\")) + to)).Expand().Replace("/", "\\");
    print("Copying {0} to {1}... ", from, to);

    try {
      ExitCode status = -1;
      if (File.Exists(from)) status = CopyFile(from, to, overwrite);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  public ExitCode CopyFile(string sourceFile, string destFile, Boolean overwrite = true) {
    try {
      var parentDir = new DirectoryInfo(Path.GetDirectoryName(destFile));
      if (!parentDir.Exists) parentDir.Create();
      File.Copy(sourceFile, destFile, overwrite);
      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  public ExitCode transplantDir(String from, String to, Boolean overwrite = true) {
    from = (from.Expand().Contains(":") ? from : ((transplantFrom == null ? null : (transplantFrom + "\\")) + from)).Expand().Replace("/", "\\");
    to = (to.Expand().Contains(":") ? to : ((transplantTo == null ? null : (transplantTo + "\\")) + to)).Expand().Replace("/", "\\");
    print("Copying {0} to {1}... ", from, to);

    try {
      ExitCode status = -1;
      status = CopyDirectory(from, to, overwrite);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  public ExitCode CopyDirectory(string sourceFolder, string destFolder, Boolean overwrite = true) {
    try {
      int iof = -1;
      while ((iof = sourceFolder.IndexOf(".jar")) != -1) {
        var archive = sourceFolder.Substring(0, iof + 4);
        var insideArchive = sourceFolder.Substring(iof + 4);
        var temp = Path.GetTempFileName() + ".unpack";
        var status = Console.batch(String.Format("unzip -qq \"{0}\" -d \"{1}\"", archive, temp));
        if (!status) { println("[FAILED]"); return status; }
        sourceFolder = temp + "\\" + insideArchive;
      }

      if (ZlpIOHelper.DirectoryExists(destFolder) && overwrite) ZlpIOHelper.DeleteDirectory(destFolder, true);
      if (!ZlpIOHelper.DirectoryExists(destFolder)) ZlpIOHelper.CreateDirectory(destFolder);

      var files = new ZlpDirectoryInfo(sourceFolder).GetFiles();
      foreach (var file in files) {
        var name = ZlpPathHelper.GetFileNameFromFilePath(file.FullName);
        var dest = ZlpPathHelper.Combine(destFolder, name);
        file.CopyTo(dest, true);
      }

      var folders = new ZlpDirectoryInfo(sourceFolder).GetDirectories();
      foreach (var folder in folders) {
        var name = ZlpPathHelper.GetFileNameFromFilePath(folder.FullName);
        var dest = ZlpPathHelper.Combine(destFolder, name);
        var status = CopyDirectory(folder.FullName, dest);
        if (!status) { println("[FAILED]"); return status; }
      }

      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  protected ExitCode deleteFile(String target) {
    target = (target.Expand().Contains(":") ? target : ((transplantTo == null ? null : (transplantTo + "\\")) + target)).Expand().Replace("/", "\\");
    print("Deleting {0}... ", target);

    try {
      ExitCode status = 0;
      if (File.Exists(target)) status = DeleteFile(target);
      if (status) println("[  OK  ]");
      return status;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }

  public ExitCode DeleteFile(string targetFile) {
    try {
      File.Delete(targetFile);
      return 0;
    } catch (Exception ex) {
      println("[FAILED]");
      println(ex);
      return -1;
    }
  }
}

public abstract class Conn : Base {
  public FileInfo file;
  public DirectoryInfo dir;

  public Conn() : this((DirectoryInfo)null) {}

  public Conn(FileInfo file) {
    this.file = file;
    this.dir = file == null ? null : file.Directory;
  }

  public Conn(DirectoryInfo dir) {
    this.file = null;
    this.dir = dir;
  }

  public abstract bool accept();
  public virtual void init() {} // executed right before an action is called reflectively

  public virtual DirectoryInfo root { get {
    return dir;
  } }

  public static Dictionary<String, String> env { get {
    return Config.env;
  } }

  [Action, DontTrace]
  public ExitCode menu() {
    if (Config.rawTarget == "") {
      var menuitems = this.menuitems();
      menuitems.ForEach(menuitem => println(menuitem));
      return menuitems.Count() == 0 ? -1 : 0;
    } else {
      var hotkey = Config.sanitizedRawTarget;
      if (Config.args.Count() == 0) {
        Config.rawTarget = "";
        Config.target = "";
        Config.rawCommandLine = "";
      } else {
        Config.rawTarget = Config.args[0];
        Config.target = Config.args[0];
        Config.rawCommandLine = String.Join(" ", Config.args.Skip(1).ToArray());
      }
      var runtime = this.menuitem(hotkey);
      if (runtime != null) return runtime();
      else return -1;
    }
  }
}

[Connector(name = "universal", priority = 0, description =
  "Accepts any target, serves as a fallback for universally provided services such as `console` and `prompt`.")]

public class Universal : Conn {
  public Universal() : base() {}
  public Universal(FileInfo file) : base(file) {}
  public Universal(DirectoryInfo dir) : base(dir) {}

  public override bool accept() {
    return true;
  }

  [Action, DontTrace]
  public virtual ExitCode console() {
    return Console.interactive("mycmd.exe", home: dir);
  }

  [Action, DontTrace]
  public virtual ExitCode prompt() {
    println(dir.ToString() + ">");
    return 0;
  }

  [Action]
  public virtual ExitCode open() {
    return Console.ui(file != null ? file.FullName : dir.FullName);
  }
}

public abstract class Prj : Universal {
  public Prj() : base() {}
  public Prj(FileInfo file) : base(file) {}
  public Prj(DirectoryInfo dir) : base(dir) { this.dir = this.dir ?? (project == null ? null : new DirectoryInfo(project)); }

  public virtual String project { get { return null; } }
  protected override String transplantFrom { get { return project; } }

  public override bool accept() {
    if (Config.verbose) println("project = {0}, dir = {1}", project.Expand(), dir.FullName);
    return project != null && dir.IsChildOrEquivalentTo(project);
  }

  public override DirectoryInfo root { get {
    if (project != null) {
      return new DirectoryInfo(project);
    }

    return dir;
  } }

//  [Action, DontTrace]
//  public override ExitCode console() {
//    return Console.interactive("mycmd.exe", home: root);
//  }

  [Action]
  public virtual ExitCode makeTestSuite(Arguments arguments) {
    var suite = arguments.Last();
    var dotTestName = ".test" + "." + suite;
    var dotTest = new FileInfo(root + "\\" + dotTestName);
    if (!setTestSuite(new Arguments(new List<String>{suite}))) return -1;

    if (suite == "failed") {
      arguments = new Arguments(new List<String>{"failed", "failed"});
    } else if (suite == "succeeded") {
      arguments = new Arguments(new List<String>{"succeeded", "succeeded"});
    } else {
      if (dotTest.Exists) {
        print("suite " + suite + " already exists. overwrite? ");
        var result = readln().ToLower();
        var confirmed = result == "" || result == "y" || result == "yes" || result == "yep";
        if (!confirmed) { return -1; }
      }
    }

    File.WriteAllLines(dotTest.FullName, new String[]{});
    println("created suite " + suite);

    var wildcards = new List<String>{Config.target}.Concat(arguments.Take(arguments.Count() - 1)).ToList();
    if (wildcards.First() == Path.GetFullPath(".")) wildcards = wildcards.Skip(1).ToList();
    addTestSuiteTests(suite, wildcards);

    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode addFilesToTestSuite(Arguments arguments) {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    var wildcards = new List<String>{Config.target}.Concat(arguments).ToList();
    if (wildcards.First() == Path.GetFullPath(".")) wildcards = wildcards.Skip(1).ToList();
    addTestSuiteTests(suite, wildcards, false);

    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode removeFilesFromTestSuite(Arguments arguments) {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    var wildcards = new List<String>{Config.target}.Concat(arguments).ToList();
    if (wildcards.First() == Path.GetFullPath(".")) wildcards = wildcards.Skip(1).ToList();
    removeTestSuiteTests(suite, wildcards);

    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode getTestSuite() {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    println(suite);
    env["currentTestSuite"] = suite;
    var path = @"Software\Far2\SavedDialogHistory\MykeTestSuites";
    Registry.CurrentUser.DeleteSubKey(path, false);
    var reg = Registry.CurrentUser.OpenSubKey(path, true) ?? Registry.CurrentUser.CreateSubKey(path);
    path = "HKEY_CURRENT_USER\\" + path;
    Registry.SetValue(path, "Lines", getTestSuites().ToArray(), RegistryValueKind.MultiString);

    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode setTestSuite(Arguments arguments) {
    var suite = arguments.Last();
    var dotProfile = new FileInfo(root + "\\.profile");
    File.WriteAllText(dotProfile.FullName, suite);
    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode listTestSuiteAllTests() {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    var tests = getTestSuiteAllTests(suite);
    if (tests == null || tests.Count() == 0) { println(suite + " does not have any tests"); return -1; }

    tests.ForEach(test => println(test));
    //println(suite + " has " + tests.Count + " test" + (tests.Count == 1 ? "" : "s"));
    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode listTestSuiteFailedTests() {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    var tests = getTestSuiteFailedTests(suite);
    if (tests == null || tests.Count() == 0) { println(suite + " does not have any failed tests"); return -1; }

    tests.ForEach(test => println(test));
    //println(suite + " has " + tests.Count + " failed test" + (tests.Count == 1 ? "" : "s"));
    return 0;
  }

  [Action, DontTrace]
  public virtual ExitCode listTestSuiteSucceededTests() {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    var tests = getTestSuiteSucceededTests(suite);
    if (tests == null || tests.Count() == 0) { println(suite + " does not have any succeeded tests"); return -1; }

    tests.ForEach(test => println(test));
    //println(suite + " has " + tests.Count + " succeeded test" + (tests.Count == 1 ? "" : "s"));
    return 0;
  }

  public virtual String getCurrentTestSuite() {
    var dotProfile = new FileInfo(root + "\\.profile");
    return dotProfile.Exists ? File.ReadAllText(dotProfile.FullName) : null;
  }

  public virtual List<String> getTestSuites() {
    return root.GetFiles(".test.*").Select(fi => fi.Extension).Select(s => s.StartsWith(".") ? s.Substring(1) : s).ToList();
  }

  public virtual List<String> getTestSuiteAllTests(String profile) {
    var dotTestName = ".test" + (String.IsNullOrEmpty(profile) ? "" : "." + profile);
    var dotTest = new FileInfo(root + "\\" + dotTestName);
    if (!dotTest.Exists) {
      println("error: " + dotTestName + " file not found");
      return null;
    }

    var fs_toTest = File.ReadAllLines(dotTest.FullName).ToList();
    return fs_toTest.Select(f_toTest => {
      var iof = f_toTest.IndexOf("#");
      if (iof != -1) f_toTest = f_toTest.Substring(0, iof);
      return f_toTest.Trim();
    }).Where(f_toTest => f_toTest != String.Empty).ToList();
  }

  public virtual List<String> getTestSuiteFailedTests(String profile) {
    return new List<String>{"not supported"};
  }

  public virtual List<String> getTestSuiteSucceededTests(String profile) {
    return new List<String>{"not supported"};
  }

  public virtual List<String> calculateTestSuiteTests(String profile) {
    return null;
  }

  public virtual void addTestSuiteTests(String profile, List<String> wildcards, bool removeNonMatching = false) {
    var dotTestName = ".test" + "." + profile;
    var dotTest = new FileInfo(root + "\\" + dotTestName);
    var existingTests = dotTest.Exists ? File.ReadAllLines(dotTest.FullName).ToList() : new List<String>();
    println("found " + existingTests.Count + " existing " + profile + " tests");

    var tests = new List<String>();
    wildcards.ForEach(wildcard => {
      wildcard = wildcard.Replace("/", "\\");

      println(wildcard);
      var relativeTo = Path.GetDirectoryName(wildcard);
      if (!wildcard.Contains("\\")) {
        relativeTo = Path.GetFullPath(new DirectoryInfo(".").FullName);
        if (relativeTo.EndsWith("\\")) relativeTo = relativeTo.Substring(0, relativeTo.Length - 1);
        wildcard = relativeTo + "\\" + wildcard;
      } else if (!wildcard.Contains(":")) {
        relativeTo = root.FullName;
        if (relativeTo.EndsWith("\\")) relativeTo = relativeTo.Substring(0, relativeTo.Length - 1);
        wildcard = relativeTo + "\\" + wildcard;
      }

      if (wildcard.Contains("*") || wildcard.Contains("?")) {
        wildcard = wildcard.Substring(relativeTo.Length + 1);
        var flags = wildcard.Contains("\\") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var dir = new DirectoryInfo(relativeTo);
        if (dir.Exists) {
          dir.GetFiles(wildcard, flags).ToList().ForEach(fi => tests.Add(Path.GetFullPath(fi.FullName)));
          dir.GetDirectories(wildcard, flags).ToList().ForEach(di => tests.Add(Path.GetFullPath(di.FullName)));
        }
      } else if (wildcard == relativeTo + "\\refresh") {
        var calculated = calculateTestSuiteTests(profile);
        if (calculated != null) tests.AddRange(calculated);
      } else if (wildcard == relativeTo + "\\failed") {
        var failed = getTestSuiteFailedTests(profile);
        if (failed.Count() != 1 || failed[0] != "not supported") tests.AddRange(failed);
      } else if (wildcard == relativeTo + "\\succeeded") {
        var succeeded = getTestSuiteSucceededTests(profile);
        if (succeeded.Count() != 1 || succeeded[0] != "not supported") tests.AddRange(succeeded);
      } else {
        tests.Add(wildcard);
      }
    });

    println("adding " + tests.Count + " " + profile + " tests");
    var newTests = tests.Where(test => existingTests.Where(existingTest => existingTest.Contains(test)).Count() == 0).ToList();
    if (newTests.Count() == 0) println("none of them are new");
    else if (newTests.Count() == tests.Count()) println("all of them are new");
    else println(newTests.Count() + " of them are new: " + String.Join(", ", newTests.ToArray()));
    var obsoleteTests = removeNonMatching ? existingTests.Where(existingTest => tests.Where(test => existingTest.Contains(test)).Count() == 0).ToList() : new List<String>();
    if (removeNonMatching) {
      if (obsoleteTests.Count() == 0) println("none of the existing tests are obsolete");
      else if (obsoleteTests.Count() == tests.Count()) println("all of the existing tests are obsolete");
      else println(obsoleteTests.Count() + " of the existing tests are obsolete: " + String.Join(", ", obsoleteTests.ToArray()));
    }

    if (newTests.Count() != 0 || obsoleteTests.Count() != 0) {
      existingTests.AddRange(newTests);
      existingTests.RemoveAll(existingTest => obsoleteTests.Contains(existingTest));

      File.WriteAllLines(dotTest.FullName, existingTests);
      println("wrote " + dotTest.FullName.Substring(root.FullName.Length + 1));
    }
  }

  public virtual void removeTestSuiteTests(String profile, List<String> wildcards) {
    var dotTestName = ".test" + "." + profile;
    var dotTest = new FileInfo(root + "\\" + dotTestName);
    var existingTests = dotTest.Exists ? File.ReadAllLines(dotTest.FullName).ToList() : new List<String>();
    println("found " + existingTests.Count + " existing " + profile + " tests");

    var removedTests = wildcards.SelectMany(wildcard => {
      wildcard = wildcard.Trim();
      if (!wildcard.StartsWith("*")) wildcard = "*" + wildcard;
      if (!wildcard.EndsWith("*")) wildcard = wildcard + "*";
      return existingTests.Where(existingTest => Operators.LikeString(existingTest, wildcard, CompareMethod.Text));
    });
    if (removedTests.Count() == 0) println("none of the existing tests are to be removed");
    else if (removedTests.Count() == existingTests.Count()) println("all of the existing tests are to be removed");
    else println(removedTests.Count() + " of the existing tests are to be removed: " + String.Join(", ", removedTests.ToArray()));

    if (removedTests.Count() != 0) {
      existingTests.RemoveAll(existingTest => removedTests.Contains(existingTest));
      File.WriteAllLines(dotTest.FullName, existingTests);
      println("wrote " + dotTest.FullName.Substring(root.FullName.Length + 1));
    }
  }

  [Action]
  public virtual ExitCode runTest() {
    var suite = getCurrentTestSuite();
    if (suite == null) { println("there is no test suite associated with this project"); return -1; }

    println("don't know how to run test suite " + suite);
    return -1;
  }
}

public abstract class Git : Prj {
  public Git() : base((DirectoryInfo)null) {}
  public Git(FileInfo file) : base(file) {}
  public Git(DirectoryInfo dir) : base(dir) {}

  public override DirectoryInfo root { get {
    if (project != null) {
      return new DirectoryInfo(project);
    }

    if (repo != null) {
      return repo;
    }

    return dir;
  } }

  public DirectoryInfo repo { get {
    // todo. do we need to cache this?
    return detectRepo();
  } }

  public virtual DirectoryInfo detectRepo() {
    var wannabe = file != null ? file.Directory : dir;
    if (wannabe.IsChildOrEquivalentTo("%SCRIPTS_HOME%".Expand())) return new DirectoryInfo(@"%SOFTWARE%".Expand());

    while (wannabe != null) {
      var gitIndex = wannabe.GetDirectories().FirstOrDefault(child => child.Name == ".git");
      if (gitIndex != null) return wannabe;
      wannabe = wannabe.Parent;
    }

    return null;
  }

  public virtual bool verifyRepo() {
    if (repo == null) {
      Console.println("error: {0} is not under Git repository", file != null ? file.FullName : dir.FullName);
      Console.print("Create the repo with the project root (y/n)? ");
      var answer = Console.readln();
      if (answer == "" || answer.ToLower() == "y" || answer.ToLower() == "yes") {
        Console.batch("git init", home: root);
        return true;
      } else {
        return false;
      }
    } else {
      return true;
    }
  }

  private Repository _gitRepo;
  public Repository gitRepo { get {
    if (repo == null)
      return null;

    if (_gitRepo == null) {
      _gitRepo = new Repository(repo.FullName);
    }

    return _gitRepo;
  } }

  [Action, DontTrace]
  public virtual ExitCode smartCommit() {
    if (dir.IsChildOrEquivalentTo("%SCRIPTS_HOME%".Expand())) {
      var status = Console.batch("save-settings.bat");
      if (status != 0) return -1;
    }

    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit commit \"{0}\"", repo.GetRealPath().FullName));
  }

  [Action, DontTrace]
  public virtual ExitCode smartLogall() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit log \"{0}\"", repo.GetRealPath().FullName));
  }

  [Action, DontTrace]
  public virtual ExitCode smartLogthis() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit log \"{0}\"", (file != null ? (FileSystemInfo)file : dir).GetRealPath().FullName));
  }

  [Action, DontTrace]
  public virtual ExitCode smartBlame() {
    if (!verifyRepo()) return -1;
    return Console.ui(String.Format("tgit blame \"{0}\"", (file != null ? (FileSystemInfo)file : dir).GetRealPath().FullName));
  }

  [Action, DontTrace]
  public virtual ExitCode smartLog() {
    return smartLogall();
  }

  [Action, DontTrace]
  public virtual ExitCode push() {
    if (!verifyRepo()) return -1;
    return Console.interactive("git push " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace, MenuItem(description = "Push to origin", priority = 10000)]
  public virtual ExitCode smartPush() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (branch == "") branch = getCurrentBranch();
    var result = Console.batch("git push origin " + branch, home: repo.GetRealPath());
    if (!result) {
      // todo. detect the real reason of the failure
      // and show the offending commits (local and remote ones)
      println();
      println("Push has failed (presumably because of a non-fast-forward update).");
      print("Do you want to force it? ");
      var answer = System.Console.ReadLine();
      if (answer == null) return result;
      answer = answer.ToLower();
      if (answer == "y" || answer == "yes" || answer == "yep") {
        println();
        return Console.batch("git push origin +" + branch, home: repo.GetRealPath());
      } else {
        return result;
      }
    }
    return result;
  }

  [Action, DontTrace, MenuItem(description = "Pull from origin", priority = 9999)]
  public virtual ExitCode smartPullFromOrign() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (branch == "") branch = getCurrentBranch();
    return Console.batch("git pull origin " + branch, home: repo.GetRealPath());
  }

  [Action, DontTrace, MenuItem(description = "Pull from upstream", priority = 9998)]
  public virtual ExitCode smartPullFromUpstream() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (branch == "") branch = getCurrentBranch();
    return Console.batch("git pull upstream " + branch, home: repo.GetRealPath());
  }

  [Action, DontTrace, MenuItem(hotkey = "z", description = "Submit pull request", priority = 190)]
  public virtual ExitCode smartPullRequest() {
    if (!verifyRepo()) return -1;
    var gitStatus = getCurrentStatus();
    if (gitStatus != null && gitStatus.Contains("nothing to commit")) {
      println("Nothing to commit");
      var branch = Config.sanitizedRawTarget;
      if (branch == "") branch = getCurrentBranch();
      var url = getBranchPullRequestUrl(branch);
      if (url == null) return -1;
      return smartPush() && Console.ui(url);
    } else {
      return smartCommit();
    }
  }

  [Action, DontTrace, MenuItem(hotkey = "a", description = "Show branch at GitHub", priority = 120)]
  public virtual ExitCode smartShowBranchAtGithub() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (branch == "") branch = getCurrentBranch();
    var url = getBranchUrl(branch);
    if (url == null) return -1;
    // return Console.batch("git push origin +" + getCurrentBranch(), home: repo.GetRealPath()) && Console.ui(url);
    return Console.ui(url);
  }

  [Action, DontTrace, MenuItem(hotkey = "q", description = "Show commit at GitHub", priority = 110)]
  public virtual ExitCode smartShowCommitAtGithub() {
    if (!verifyRepo()) return -1;
    var commit = Config.sanitizedRawTarget;
    if (commit == "") commit = getCurrentHead();
    var url = getCommitUrl(commit);
    if (url == null) return -1;
    // return Console.batch("git push origin +" + getCurrentBranch(), home: repo.GetRealPath()) && Console.ui(url);
    return Console.ui(url);
  }

  [Action, DontTrace, MenuItem(hotkey = "x", description = "Show file at GitHub (current revision)", priority = 90)]
  public virtual ExitCode smartShowFileAtGithubRevisionAware() {
    if (!verifyRepo()) return -1;
    var file = Config.rawTarget;
    var url = getFileUrlRevisionAware(file);
    if (url == null) return -1;
    // return Console.batch("git push origin +" + getCurrentBranch(), home: repo.GetRealPath()) && Console.ui(url);
    return Console.ui(url);
  }

  [Action, DontTrace, MenuItem(hotkey = "c", description = "Show file at GitHub (revision-agnostic)", priority = 80)]
  public virtual ExitCode smartShowFileAtGithubRevisionAgnostic() {
    if (!verifyRepo()) return -1;
    var file = Config.rawTarget;
    var url = getFileUrlRevisionAgnostic(file);
    if (url == null) return -1;
    // return smartPush() && Console.ui(url);
    return Console.ui(url);
  }

  [Action, DontTrace]
  public virtual ExitCode pull() {
    if (!verifyRepo()) return -1;
    return Console.batch("git pull", home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartPull() {
    if (!verifyRepo()) return -1;
    var remoteAndBranch = Config.rawCommandLine;
    if (remoteAndBranch == "") remoteAndBranch += "origin";
    if (!remoteAndBranch.Contains(" ")) remoteAndBranch += (" " + getCurrentBranch());
    return Console.batch("git pull " + remoteAndBranch, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode branch() {
    if (!verifyRepo()) return -1;
    return Console.batch("git branch " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartBranchRename() {
    if (!verifyRepo()) return -1;
    if (Config.args.Count() < 1) return -1;
    var oldname = Config.sanitizedRawTarget;
    var newname = Config.args[0];
    return Console.batch("git branch -m " + oldname + " " + newname, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartBranchLocalDelete() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    ExitCode result = 0;
    if (branch == getCurrentBranch()) result = Console.batch("git checkout master", home: repo.GetRealPath());
    return result && Console.batch("git branch -D " + branch, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartBranchRemoteDelete() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (!branch.StartsWith("remotes/")) {
      var result = smartBranchLocalDelete();
      return result && Console.batch("git push origin :" + branch, home: repo.GetRealPath());
    } else {
      branch = branch.Substring("remotes/".Length);
      var iof = branch.IndexOf("/");
      var remote = branch.Substring(0, iof);
      branch = branch.Substring(iof + 1);
      return Console.batch("git push " + remote + " :" + branch, home: repo.GetRealPath());
    }
  }

  [Action, DontTrace]
  public virtual ExitCode merge() {
    if (!verifyRepo()) return -1;
    return Console.batch("git merge " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartMerge() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    return Console.batch("git merge " + branch, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode rebase() {
    if (!verifyRepo()) return -1;
    return Console.batch("git rebase " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartRebase() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    return Console.batch("git rebase " + branch, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode cherryPick() {
    if (!verifyRepo()) return -1;
    return Console.batch("git cherry-pick " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartCherryPick() {
    if (!verifyRepo()) return -1;
    var commit = Config.sanitizedRawTarget;
    return Console.batch("git cherry-pick " + commit, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode checkout() {
    if (!verifyRepo()) return -1;
    return Console.batch("git checkout " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartBranchNewSelect() {
    if (!verifyRepo()) return -1;
    var result = Console.batch("git branch " + Config.args[0] + " " + Config.rawTarget, home: repo.GetRealPath());
    return result && Console.batch("git checkout " + Config.args[0]);
  }

  [Action, DontTrace]
  public virtual ExitCode smartCheckout() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (branch.StartsWith("remotes/")) return Console.batch("git checkout -t " + branch, home: repo.GetRealPath());
    else return Console.batch("git checkout " + branch, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode remote() {
    if (!verifyRepo()) return -1;
    return Console.batch("git remote " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode reset() {
    if (!verifyRepo()) return -1;
    return Console.batch("git reset " + Config.rawCommandLine, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartMixedReset() {
    if (!verifyRepo()) return -1;
    var commit = Config.sanitizedRawTarget;
    return Console.batch("git reset --mixed " + commit, home: repo.GetRealPath());
  }

  [Action, DontTrace]
  public virtual ExitCode smartHardReset() {
    if (!verifyRepo()) return -1;
    var commit = Config.sanitizedRawTarget;
    return Console.batch("git reset --hard " + commit, home: repo.GetRealPath());
  }

  public virtual String getGithubUrl(String remote) {
    var remotes = Console.eval("git remote -v", home: repo.GetRealPath());
    var line = remotes.lines.Where(line2 => line2.StartsWith(remote)).FirstOrDefault();
    if (line == null) return null;
    line = line.Substring(remote.Length).Trim();
    line = line.Substring(0, line.LastIndexOf("(") - 1).Trim();
    // https://github.com/scalamacros/kepler/pull/new/topic/reflection
    var url1 = line;
    var re1 = "^git://github.com/(?<user>.*?)/(?<repo>.*).git$";
    var m1 = Regex.Match(url1, re1);
    if (m1.Success) {
      return m1.Result("https://github.com/${user}/${repo}");
    } else {
      var re2 = "^git@github.com:(?<user>.*?)/(?<repo>.*).git$";
      var m2 = Regex.Match(url1, re2);
      if (m2.Success) {
        return m2.Result("https://github.com/${user}/${repo}");
      } else {
        return null;
      }
    }
  }

  public virtual String getTargetOfPullRequest(String branch) {
    return null;
  }

  public virtual String getBranchPullRequestUrl(String branch) {
    String remote = null;
    if (branch.StartsWith("remotes/")) {
      branch = branch.Substring("remotes/".Length);
      remote = branch.Substring(0, branch.IndexOf("/") - 1);
      branch = branch.Substring(branch.IndexOf("/") + 1);
    } else {
      remote = "origin";
    }

    var url = getGithubUrl(remote);
    if (url == null) return null;
    else {
      var target = getTargetOfPullRequest(branch);
      var prefix = target == null ? "" : (target + "...");
      return url + "/pull/new/" + prefix + branch;
    }
  }

  public virtual String getCommitUrl(String commit) {
    var url = getGithubUrl("origin");
    if (url == null) return null;
    else return url + "/commit/" + commit;
  }

  public virtual String getFileUrlRevisionAgnostic(String file) {
    var url = getGithubUrl("origin");
    if (url == null) return null;

    var linum = file.Contains("#L") ? file.Substring(file.IndexOf("#L") + 2) : null;
    if (linum != null) file = file.Substring(0, file.IndexOf("#L"));
    var normalizedFile = file.GetRealPath().Replace("\\", "/");
    var normalizedRoot = root.FullName.GetRealPath().Replace("\\", "/");

    var nomorehacks = false;
    Action<String, String> hackFile = (Action<String, String>)((String from, String to) => { if (normalizedFile.StartsWith(from)) normalizedFile = to + normalizedFile.Substring(from.Length); });
    Action<String, String> hackRoot = (Action<String, String>)((String from, String to) => { if (!nomorehacks && normalizedRoot.StartsWith(from)) { normalizedRoot = to; nomorehacks = true; } });
    hackRoot("D:/Dropbox/Software/Sublime/Packages/SublimeEnsime", "D:/Dropbox/Software/Sublime/Packages/SublimeEnsime");
    hackRoot("D:/Dropbox/Software/Sublime/Packages/sublime-ensime", "D:/Dropbox/Software/Sublime/Packages/sublime-ensime");
    hackRoot("D:/Dropbox/Software/Sublime/", "D:/Dropbox/Software/Sublime/");
    hackFile("C:/Program Files (x86)/scripts/", "C:/Program Files (x86)/scripts/Scripts/Scripts/");

    if (normalizedFile.StartsWith(normalizedRoot)) normalizedFile = normalizedFile.Substring(normalizedRoot.Length);
    if (normalizedFile.StartsWith("/")) normalizedFile = normalizedFile.Substring(1);
    return url + "/blob/" + getCurrentBranch() + "/" + normalizedFile + (linum == null ? null : ("#L" + linum));
  }

  public virtual String getFileUrlRevisionAware(String file) {
    var url = getGithubUrl("origin");
    if (url == null) return null;

    var linum = file.Contains("#L") ? file.Substring(file.IndexOf("#L") + 2) : null;
    if (linum != null) file = file.Substring(0, file.IndexOf("#L"));
    var normalizedFile = file.GetRealPath().Replace("\\", "/");
    var normalizedRoot = root.FullName.GetRealPath().Replace("\\", "/");

    var nomorehacks = false;
    Action<String, String> hackFile = (Action<String, String>)((String from, String to) => { if (normalizedFile.StartsWith(from)) normalizedFile = to + normalizedFile.Substring(from.Length); });
    Action<String, String> hackRoot = (Action<String, String>)((String from, String to) => { if (!nomorehacks && normalizedRoot.StartsWith(from)) { normalizedRoot = to; nomorehacks = true; } });
    hackRoot("D:/Dropbox/Software/Sublime/Packages/SublimeEnsime", "D:/Dropbox/Software/Sublime/Packages/SublimeEnsime");
    hackRoot("D:/Dropbox/Software/Sublime/Packages/sublime-ensime", "D:/Dropbox/Software/Sublime/Packages/sublime-ensime");
    hackRoot("D:/Dropbox/Software/Sublime/", "D:/Dropbox/Software/Sublime/");
    hackFile("C:/Program Files (x86)/scripts/", "C:/Program Files (x86)/scripts/Scripts/Scripts/");

    if (normalizedFile.StartsWith(normalizedRoot)) normalizedFile = normalizedFile.Substring(normalizedRoot.Length);
    if (normalizedFile.StartsWith("/")) normalizedFile = normalizedFile.Substring(1);
    return url + "/blob/" + getCurrentHead() + "/" + normalizedFile + (linum == null ? null : ("#L" + linum));
  }

  public virtual String getBranchUrl(String branch) {
    String remote = null;
    if (branch.StartsWith("remotes/")) {
      branch = branch.Substring("remotes/".Length);
      remote = branch.Substring(0, branch.IndexOf("/") - 1);
      branch = branch.Substring(branch.IndexOf("/") + 1);
    } else {
      remote = "origin";
    }

    var url = getGithubUrl(remote);
    if (url == null) return null;
    else return url + "/tree/" + branch;
  }

  public virtual String getCurrentBranch() {
    if (gitRepo == null) return null;
    return gitRepo.Head.Name;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartCurrentBranch() {
    if (!verifyRepo()) return -1;
    var branch = getCurrentBranch();
    if (branch == null) return -1;
    println(branch);
    return 0;
  }

  public virtual String getCurrentHead() {
    if (gitRepo == null) return null;
    return gitRepo.Head.Tip.Sha;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartCurrentHead() {
    if (!verifyRepo()) return -1;
    var commit = getCurrentBranch();
    if (commit == null) return -1;
    println(commit);
    return 0;
  }

  public virtual String getCurrentCommit() {
    if (gitRepo == null) return null;
    return showCommit(gitRepo.Head.Tip, absoluteTime: true);
  }

  public virtual String getCurrentStatus() {
    var status = Console.eval("git status", home: repo.GetRealPath());
    if (!status.status) return null;
    return String.Join("\r\n", status.lines.ToArray());
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartShowCommit() {
    if (!verifyRepo()) return -1;
    var commit = Config.sanitizedRawTarget;
    var gitCommit = gitRepo.Lookup<Commit>(commit);
    var gitParentCommit = gitCommit.Parents.First();
    var diff = gitRepo.Diff.Compare(gitParentCommit.Tree, gitCommit.Tree);
    println(diff.Patch);
    return 0;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartShowCommitStructure() {
    if (!verifyRepo()) return -1;
    var commit = Config.sanitizedRawTarget;
    var gitCommit = gitRepo.Lookup<Commit>(commit);
    var gitParentCommit = gitCommit.Parents.First();
    var diff = gitRepo.Diff.Compare(gitParentCommit.Tree, gitCommit.Tree);
    diff.OrderBy(cs => cs.Path).ToList().ForEach(cs => {
      println("{0} {1}", cs.Status.ToString()[0], cs.Path);
    });
    return 0;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartShowCommitStructureEntryPatch() {
    if (!verifyRepo()) return -1;
    var raw = Config.rawCommandLine.Trim();
    var commit = raw.Substring(0, raw.IndexOf(" "));
    var entry = raw.Substring(raw.IndexOf(" ") + 1);
    var gitCommit = gitRepo.Lookup<Commit>(commit);
    var gitParentCommit = gitCommit.Parents.First();
    var diff = gitRepo.Diff.Compare(gitParentCommit.Tree, gitCommit.Tree);
    var changes = diff[entry];
    println(changes.Patch);
    return 0;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartShowCommitStructureEntryMerge() {
    if (!verifyRepo()) return -1;
    var raw = Config.rawCommandLine.Trim();
    var commit = raw.Substring(0, raw.IndexOf(" "));
    var entry = raw.Substring(raw.IndexOf(" ") + 1);
    var after = gitRepo.Lookup<Commit>(commit).Tree[entry];
    var before = gitRepo.Lookup<Commit>(commit).Parents.First().Tree[entry];
    if (before == null) return -1;
    entry = entry.Replace("/", "\\");
    var shortName = entry.LastIndexOf("\\") == -1 ? entry : entry.Substring(entry.LastIndexOf("\\") + 1);
    var afterContent = Encoding.UTF8.GetString(gitRepo.Lookup<Blob>(after.Target.Sha).Content);
    var afterTempName = Path.GetTempPath() + shortName + "-child-" + gitRepo.Lookup<Commit>(commit).Sha.Substring(0, 10);
    File.WriteAllText(afterTempName, afterContent);
    var beforeContent = Encoding.UTF8.GetString(gitRepo.Lookup<Blob>(before.Target.Sha).Content);
    var beforeTempName = Path.GetTempPath() + shortName + "-parent-" + gitRepo.Lookup<Commit>(commit).Parents.First().Sha.Substring(0, 10);
    File.WriteAllText(beforeTempName, beforeContent);
    return Console.ui("mydiff " + beforeTempName + " " + afterTempName);
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartShowCommitStructureEntryCopy() {
    if (!verifyRepo()) return -1;
    var raw = Config.rawCommandLine.Trim();
    var commit = raw.Substring(0, raw.IndexOf(" "));
    var entry = raw.Substring(raw.IndexOf(" ") + 1);
    var target = entry.Substring(entry.IndexOf(" ") + 1);
    target = target.Replace("/", "\\");
    entry = entry.Substring(0, entry.IndexOf(" "));
    entry = entry.Replace("/", "\\");
    if (target.EndsWith(":")) target += "\\";
    if (target.EndsWith("\\")) {
      var shortName = entry.LastIndexOf("\\") == -1 ? entry : entry.Substring(entry.LastIndexOf("\\") + 1);
      target += shortName;
    }
    var after = gitRepo.Lookup<Commit>(commit).Tree[entry];
    var afterContent = Encoding.UTF8.GetString(gitRepo.Lookup<Blob>(after.Target.Sha).Content);
    File.WriteAllText(target, afterContent);
    return 0;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartShowCommitFilesystem() {
    // todo. implement virtual filesystem!
    return smartShowCommitStructure();
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartListCommits() {
    if (!verifyRepo()) return -1;
    gitRepo.Commits.Take(50).ToList().ForEach(commit => {
      if (commit.Sha == gitRepo.Head.Tip.Sha) print("* ");
      println(showCommit(commit, absoluteTime: false));
    });
    return 0;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartListBranchCommits() {
    if (!verifyRepo()) return -1;
    var branch = Config.sanitizedRawTarget;
    if (branch.Trim() == "") branch = getCurrentBranch();
    if (branch.StartsWith("remotes/")) branch = "refs/" + branch;
    gitRepo.Branches[branch].Commits.Take(50).ToList().ForEach(commit => {
      if (commit.Sha == gitRepo.Head.Tip.Sha) print("* ");
      println(showCommit(commit, absoluteTime: false));
    });
    return 0;
  }

  [Action, DontTrace, Meaningful]
  public virtual ExitCode smartListBranches() {
    if (!verifyRepo()) return -1;
    return Console.batch("git branch -a", home: repo.GetRealPath());
  }

  private String showCommit(Commit commit, Boolean absoluteTime = true) {
    String s_authortime = null;
    if (absoluteTime) s_authortime = commit.Author.When.ToString();
    else {
      Func<DateTime, String> showRelativeTime = (DateTime dt) => {
        // That's why I love StackOverflow so much
        // http://stackoverflow.com/questions/11/calculating-relative-time

        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 0) return "not yet";
        if (delta < 1 * MINUTE) return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
        if (delta < 2 * MINUTE) return "a minute ago";
        if (delta < 45 * MINUTE) return ts.Minutes + " minutes ago";
        if (delta < 90 * MINUTE) return "an hour ago";
        if (delta < 24 * HOUR) return ts.Hours + " hours ago";
        if (delta < 48 * HOUR) return "yesterday";
        if (delta < 30 * DAY) return ts.Days + " days ago";
        if (delta < 12 * MONTH) {
          int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
          return months <= 1 ? "one month ago" : months + " months ago";
        } else {
          int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
          return years <= 1 ? "one year ago" : years + " years ago";
        }
      };

      s_authortime = showRelativeTime(commit.Author.When.UtcDateTime);
    }
    return String.Format("{0} {1} ({2}, {3})", commit.Sha.Substring(0, 10), commit.MessageShort, commit.Committer.Name, s_authortime);
  }
}
