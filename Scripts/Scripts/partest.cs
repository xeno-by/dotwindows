using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  public static int Main(String[] args) {
    if (!Directory.Exists("%PROJECTS%/Kepler/build/locker/classes/partest".Expand())) {
      var exitCode = TransplantPartest();
      if (exitCode != 0) return exitCode;
    }

    return RunPartest(args);
  }

  public static int TransplantPartest() {
    var process = new Process();
    process.StartInfo.FileName = "myke.exe";
    process.StartInfo.WorkingDirectory = "%PROJECTS%/Donor".Expand();
    process.StartInfo.Arguments = "compile";
    process.StartInfo.UseShellExecute = false;
    process.Start();
    process.WaitForExit();
    return process.ExitCode;
  }

  public static int RunPartest(String[] args) {
    var classpath = "%PROJECTS%/Kepler/build/locker/classes/compiler;%PROJECTS%/Kepler/build/locker/classes/library;%PROJECTS%/Kepler/build/locker/classes/partest".Expand();
    Environment.SetEnvironmentVariable("CLASSPATH", classpath);

    var opts = new List<String>();
    opts.Add("-Xmx1024M");
    opts.Add("-Xms64M");
    opts.Add("-Dscala.home=\"%PROJECTS%/Kepler/test\"");
    opts.Add("-Dpartest.javacmd=\"java\"");
    opts.Add("-Dpartest.java_options=\"-Xmx1024M -Xms64M\"");
    opts.Add("-Dpartest.scalac_options=\"-deprecation\"");
    opts.Add("-Djava.class.path=\"%CLASSPATH%\"");
    opts.Add("-Dscala.usejavacp=\"true\"");
    opts.Add("-cp %CLASSPATH%");
    opts.Add("-classpath %CLASSPATH%");
    opts.Add("scala.tools.partest.nest.NestRunner");
    //opts.Add("--debug");
    //opts.Add("--verbose");
    opts.Add("--classpath %PROJECTS%/Kepler/build/locker/classes");
    opts.AddRange(args);

    var process = new Process();
    process.StartInfo.FileName = "java.exe";
    process.StartInfo.WorkingDirectory = "%PROJECTS%/Kepler/test".Expand();
    process.StartInfo.Arguments = String.Join(" ", opts.Select(opt => opt.Expand()).ToArray());
    process.StartInfo.UseShellExecute = false;
    process.Start();
    process.WaitForExit();
    return process.ExitCode;
  }
}

public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}
