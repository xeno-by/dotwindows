using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  public static bool kur() { return Environment.CurrentDirectory.Contains("KeplerUnderRefactoring"); }
  public static String prefix() { return (kur() ? @"%PROJECTS%\KeplerUnderRefactoring" : @"%PROJECTS%\Kepler").Expand(); }
  public static String donor() { return (kur() ? @"%PROJECTS%\DonorUnderRefactoring" : @"%PROJECTS%\Donor").Expand(); }

  public static int Main(String[] args) {
    var age1 = File.Exists((prefix() + "/build/locker/all.complete").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/all.complete").Expand()) : DateTime.MinValue;
    var age2 = File.Exists((prefix() + "/build/locker/library.complete").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/library.complete").Expand()) : DateTime.MinValue;
    var age3 = File.Exists((prefix() + "/build/locker/reflect.complete").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/reflect.complete").Expand()) : DateTime.MinValue;
    var age4 = File.Exists((prefix() + "/build/locker/compiler.complete").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/compiler.complete").Expand()) : DateTime.MinValue;
    var maxAge = age1 > age2 ? age1 : age2;
    maxAge = maxAge > age3 ? maxAge : age3;
    maxAge = maxAge > age4 ? maxAge : age4;

    var ageOfClasses = maxAge;
    age1 = File.Exists((prefix() + "/build/locker/lib/scala-library.jar").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/lib/scala-library.jar").Expand()) : DateTime.MinValue;
    age2 = File.Exists((prefix() + "/build/locker/lib/scala-reflect.jar").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/lib/scala-reflect.jar").Expand()) : DateTime.MinValue;
    age3 = File.Exists((prefix() + "/build/locker/lib/scala-compiler.jar").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/lib/scala-compiler.jar").Expand()) : DateTime.MinValue;
    age4 = File.Exists((prefix() + "/build/locker/lib/scala-partest.jar").Expand()) ? File.GetLastWriteTime((prefix() + "/build/locker/lib/scala-partest.jar").Expand()) : DateTime.MinValue;
    maxAge = age1 > age2 ? age1 : age2;
    maxAge = maxAge > age3 ? maxAge : age3;
    maxAge = maxAge > age4 ? maxAge : age4;
    if (age1 == DateTime.MinValue || age2 == DateTime.MinValue || age3 == DateTime.MinValue || age4 == DateTime.MinValue) maxAge = DateTime.MinValue;
    var ageOfLibs = maxAge;

    if (ageOfClasses > ageOfLibs) {
      var exitCode = TransplantPartest();
      if (exitCode != 0) return exitCode;
    }

    return RunPartest(args);
  }

  public static int TransplantPartest() {
    var process = new Process();
    process.StartInfo.FileName = "myke.exe";
    process.StartInfo.WorkingDirectory = donor();
    process.StartInfo.Arguments = "compile";
    process.StartInfo.UseShellExecute = false;
    process.Start();
    process.WaitForExit();
    return process.ExitCode;
  }

  public static int RunPartest(String[] args) {
    // var classpath = "%PROJECTS%/Kepler/test/files/codelib/code.jar;%PROJECTS%/Kepler/build/locker/classes/compiler;%PROJECTS%/Kepler/build/locker/classes/reflect;%PROJECTS%/Kepler/build/locker/classes/library;%PROJECTS%/Kepler/build/locker/classes/partest".Expand();
    var classpath = (prefix() + "/test/files/codelib/code.jar;" + prefix() + "/build/locker/lib/scala-compiler.jar;" + prefix() + "/build/locker/lib/scala-reflect.jar;" + prefix() + "/build/locker/lib/scala-library.jar;" + prefix() + "/build/locker/lib/scala-partest.jar").Expand();
    Environment.SetEnvironmentVariable("CLASSPATH", classpath);

    var opts = new List<String>();
    opts.Add("-Xbootclasspath/a:%CLASSPATH%");
    opts.Add("-Dpartest.scalacopts=-Xcheckinit");
    opts.Add("scala.tools.partest.nest.NestRunner");
    //opts.Add("--debug");
    //opts.Add("--verbose");
//    opts.Add("--classpath %PROJECTS%/Kepler/build/locker/classes");
    opts.Add("--buildpath " + prefix() + "/build/locker");
    opts.AddRange(args);

    var process = new Process();
    process.StartInfo.FileName = "java.exe";
    process.StartInfo.WorkingDirectory = (prefix() + "/test").Expand();
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
