// build this with "csc /t:exe /out:partest.exe /debug+ partest.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

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
}

public static class Console {
  public static void print(String format, params Object[] objs) {
    System.Console.Write(String.Format(format, objs));
  }

  public static void print(String obj) {
    System.Console.Write(obj);
  }

  public static void print(Object obj) {
    System.Console.Write(obj);
  }

  public static void println(String format, params Object[] objs) {
    System.Console.WriteLine(String.Format(format, objs));
  }

  public static void println(String obj) {
    System.Console.WriteLine(obj);
  }

  public static void println(Object obj) {
    System.Console.WriteLine(obj);
  }

  public static void println() {
    System.Console.WriteLine();
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

  private static List<String> internalEval(String command, DirectoryInfo home = null) {
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

    var p = new Process();
    p.StartInfo = psi;

    var buf = new StringBuilder();
    p.OutputDataReceived += (sender, args) => { if (args.Data != null) buf.Append(args.Data + "\r\n"); };
    p.ErrorDataReceived += (sender, args) => { if (args.Data != null) buf.Append(args.Data + "\r\n"); };

    if (p.Start()) {
      p.BeginOutputReadLine();
      p.BeginErrorReadLine();
      p.WaitForExit();
      return buf.ToString().Split(new []{"\r\n"}, StringSplitOptions.None).ToList();
    } else {
      return null;
    }
  }

  private static ExitCode cmd(String command, DirectoryInfo home = null) {
    var script = Path.GetTempFileName() + ".bat";
    File.AppendAllText(script, "@echo off" + "\r\n");
    File.AppendAllText(script, "cd /D \"" + (home ?? new DirectoryInfo(".")).FullName + "\"" + "\r\n");
    File.AppendAllText(script, command + "\r\n");
    File.AppendAllText(script, "exit /b %errorlevel%");

    var psi = new ProcessStartInfo();
    psi.FileName = script;
    psi.WorkingDirectory = (home ?? new DirectoryInfo(".")).FullName;
    psi.UseShellExecute = false;

    var p = new Process();
    p.StartInfo = psi;

    if (p.Start()) {
      p.WaitForExit();
      return p.ExitCode;
    } else {
      return -1;
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

    var p = new Process();
    p.StartInfo = psi;

    if (p.Start()) {
      return 0;
    } else {
      return -1;
    }
  }

  public static List<String> eval(String command) {
    return eval(command, null as DirectoryInfo);
  }

  public static List<String> eval(String command, String home = null) {
    return eval(command, home == null ? null as DirectoryInfo : new DirectoryInfo(home));
  }

  public static List<String> eval(String command, DirectoryInfo home = null) {
    if (home == null) {
      home = new DirectoryInfo(".");
    }

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
    }

    return cmd(command, home);
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
    }

    return cmd(command, home);
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
    }

    return shellex(command, home);
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}

public class Result {
  public FileInfo file;
  public bool passed;
  public String log;

  public static implicit operator bool(Result r) {
    return r != null && r.passed;
  }

  public static bool operator true(Result r) {
    return r != null && r.passed;
  }

  public static bool operator false(Result r) {
    return r == null || !r.passed;
  }
}

public static class Scala {
  public static String home;
  public static String compVer;
  public static String libVer;
  public static String ver;
  public static String compiler;
  public static String runner;

  static Scala() {
    home = Console.eval("where scalac.bat").FirstOrDefault();
    if (home == null) {
      throw new Exception("scalac not found");
    } else {
      compiler = Path.GetDirectoryName(home) + "\\scalac.bat";
      runner = Path.GetDirectoryName(home) + "\\scala.bat";
      home = Path.GetFullPath(Path.GetDirectoryName(home) + "\\..");
    }

    var status = Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-compiler.jar\" compiler.properties", home), home: (home + "\\lib"));
    var compVerFile = home + "\\lib\\compiler.properties";
    compVer = null as String;
    try {
      if (!status || !File.Exists(compVerFile)) {
        throw new Exception("scalac version not found: bad unzip @ scala-compiler.jar");
      } else {
        compVer = File.ReadAllLines(compVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<compVer>.*)$");
          return m.Success ? m.Result("${compVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (compVer == null) {
          throw new Exception("scalac version not found: bad jar contents @ scala-compiler.jar");
        } else {
          compVer += (" @ " + File.ReadAllLines(compVerFile).First().Substring(1));
        }
      }
    } finally {
      if (File.Exists(compVerFile)) File.Delete(compVerFile);
    }

    status = Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-library.jar\" library.properties", home), home: (home + "\\lib"));
    var libVerFile = home + "\\lib\\library.properties";
    libVer = null as String;
    try {
      if (!status || !File.Exists(libVerFile)) {
        throw new Exception("scalac version not found: bad unzip @ scala-library.jar");
      } else {
        libVer = File.ReadAllLines(libVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<libVer>.*)$");
          return m.Success ? m.Result("${libVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (libVer == null) {
          throw new Exception("scalac version not found: bad jar contents @ scala-library.jar");
        } else {
          libVer += (" @ " + File.ReadAllLines(libVerFile).First().Substring(1));
        }
      }
    } finally {
      if (File.Exists(libVerFile)) File.Delete(libVerFile);
    }

    Scala.ver = String.Compare(compVer, libVer) > 0 ? compVer : libVer;
  }
}

public static class Handlers {
  public static Result run(FileInfo file) {
    var dir = Path.GetTempFileName() + file.Name + "-obj";
    if (Directory.Exists(dir)) Directory.Delete(dir, true);
    Directory.CreateDirectory(dir);

    var home = Path.GetDirectoryName(file.FullName);
    var log_compile = Console.eval(String.Format("\"{0}\" -d \"{1}\" \"{2}\"", Scala.compiler, dir, file.Name), home);
    while (log_compile.Count > 0 && log_compile[log_compile.Count - 1].Trim() == String.Empty) log_compile.RemoveAt(log_compile.Count - 1);
    if (log_compile.Count() == 0) {
      var log_run = Console.eval(String.Format("\"{0}\" -cp {1};{2}\\scala-library.jar;{2}\\scala-compiler.jar Test", Scala.runner, dir, Scala.home), home);
      while (log_run.Count > 0 && log_run[log_run.Count - 1].Trim() == String.Empty) log_run.RemoveAt(log_run.Count - 1);
      var f_check = Path.ChangeExtension(file.FullName, "check");
      var check = File.Exists(f_check) ? File.ReadAllLines(f_check).ToList() : new List<String>();
      var ok = compare(log_run, check);
      if (!ok) {
        return new Result{file = file, passed = false, log = "  check has failed"};
      } else {
        return new Result{file = file, passed = true, log = String.Join("\r\n", log_run.ToArray())};
      }
    } else {
      return new Result{file = file, passed = false, log = String.Join("\r\n", log_compile.ToArray())};
    }
  }

  public static Result pos(FileInfo file) {
    var dir = Path.GetTempFileName() + file.Name + "-obj";
    if (Directory.Exists(dir)) Directory.Delete(dir, true);
    Directory.CreateDirectory(dir);

    var home = Path.GetDirectoryName(file.FullName);
    var log_compile = Console.eval(String.Format("\"{0}\" -d \"{1}\" \"{2}\"", Scala.compiler, dir, file.Name), home);
    while (log_compile.Count > 0 && log_compile[log_compile.Count - 1].Trim() == String.Empty) log_compile.RemoveAt(log_compile.Count - 1);
    if (log_compile.Count() == 0) {
      var f_check = Path.ChangeExtension(file.FullName, "check");
      var check = File.Exists(f_check) ? File.ReadAllLines(f_check).ToList() : new List<String>();
      var ok = compare(log_compile, check);
      if (!ok) {
        return new Result{file = file, passed = false, log = "  check has failed"};
      } else {
        return new Result{file = file, passed = true, log = String.Join("\r\n", log_compile.ToArray())};
      }
    } else {
      return new Result{file = file, passed = false, log = String.Join("\r\n", log_compile.ToArray())};
    }
  }

  public static Result neg(FileInfo file) {
    var dir = Path.GetTempFileName() + file.Name + "-obj";
    if (Directory.Exists(dir)) Directory.Delete(dir, true);
    Directory.CreateDirectory(dir);

    var home = Path.GetDirectoryName(file.FullName);
    var log_compile = Console.eval(String.Format("\"{0}\" -d \"{1}\" \"{2}\"", Scala.compiler, dir, file.Name), home);
    while (log_compile.Count > 0 && log_compile[log_compile.Count - 1].Trim() == String.Empty) log_compile.RemoveAt(log_compile.Count - 1);
    if (log_compile.Count() == 0) {
      return new Result{file = file, passed = false, log = " should not compile"};
    } else {
      var f_check = Path.ChangeExtension(file.FullName, "check");
      var check = File.Exists(f_check) ? File.ReadAllLines(f_check).ToList() : new List<String>();
      var ok = compare(log_compile, check);
      if (!ok) {
        return new Result{file = file, passed = false, log = "  check has failed"};
      } else {
        return new Result{file = file, passed = true, log = String.Join("\r\n", log_compile.ToArray())};
      }
    }
  }

  private static bool compare(List<String> expected, List<String> actual) {
    while (expected.Count > 0 && expected[expected.Count - 1].Trim() == String.Empty) expected.RemoveAt(expected.Count - 1);
    while (actual.Count > 0 && actual[actual.Count - 1].Trim() == String.Empty) actual.RemoveAt(actual.Count - 1);
    if (expected.Count != actual.Count) return false;
    for (var i = 0; i < expected.Count; ++i) {
      if (expected[i] != actual[i]) return false;
    }
    return true;
  }
}

public class Partest {
  public static void Main(String[] args) {
    var files = args.Select(arg => new FileInfo(Path.GetFullPath(arg.Replace("/", "\\")))).ToList();
    if (files.Count() == 0) return;

    Console.println("Running {0} {1} against Scala {2}...", files.Count(), files.Count() == 1 ? "test" : "tests", Scala.ver);
    Console.println("Scala home is at {0}", Scala.home);
    Console.println();

    var sw = new Stopwatch();
    sw.Start();
    var ok = 0;
    var failed = 0;
    files.ForEach(file => {
      var result = Invoke(file);
      if (!result) {
        var log = result.log;
        if (log != null) {
          var lines = log.Split(new []{"\r\n"}, StringSplitOptions.None).Take(3).ToList();
          while (lines.Count > 0 && lines[lines.Count - 1].Trim() == String.Empty) lines.RemoveAt(lines.Count - 1);
          Console.println(String.Join("\r\n", lines));
        }
      }

      var iof = file.FullName.LastIndexOf("\\");
      iof = iof <= 0 ? iof : file.FullName.LastIndexOf("\\", iof - 1);
      iof = iof <= 0 ? iof : file.FullName.LastIndexOf("\\", iof - 1);
      var shortened = file.FullName.Substring(iof).PadRight(55);
      var status = result ? "[  OK  ]" : "[FAILED]";
      if (result) ok++; else failed++;
      Console.println("testing: [...]{0}{1}", shortened, status);
    });
    sw.Stop();

    Console.println();
    Console.println(String.Format("{0} of {1} {2} were successful (elapsed time: {3})", failed == 0 ? "All" : ok.ToString(), ok + failed, ok == 1 ? "test" : "tests", sw.Elapsed));
  }

  public static Result Invoke(FileInfo file) {
    var result = ((Func<Result>)(() => {
      try {
        if (!file.Exists) {
          return new Result{file = file, passed = false, log = "  file not found"};
        } else {
          var kind = file.Directory.Name;
          var handler = typeof(Handlers).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).SingleOrDefault(m => m.Name == kind);
          if (handler == null) {
            return new Result{file = file, passed = false, log = String.Format("  handler for file type {0} not found", kind)};
          } else {
            return (Result)handler.Invoke(null, new[]{file});
          }
        }
      } catch (Exception ex) {
        return new Result{file = file, passed = false, log = "  bug in partest: " + ex.ToString()};
      }
    }))();

    var log = Path.GetDirectoryName(file.FullName) + "\\" + Path.GetFileNameWithoutExtension(file.FullName) + "-run.log";
    if (file.Exists) File.WriteAllText(log, result.log ?? String.Empty);

    return result;
  }
}