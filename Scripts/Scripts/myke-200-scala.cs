// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

[Connector(name = "scala", priority = 200, description =
  "Builds a scala program using the command-line provided in the first line of the target file.\r\n" +
  "This no-hassle approach can do the trick for simple programs, but for more complex scenarios consider using sbt.")]

public class Scala : Git {
  private Lines lines;

  public Scala(FileInfo file, Lines lines) : base(file) {
    this.lines = lines;
  }

  public virtual String compiler { get {
    var shebang = lines.ElementAtOrDefault(0) ?? "";
    var r = new Regex("^\\s*//\\s*build\\s+this\\s+with\\s+\"(?<commandline>.*)\"\\s*$");
    var m = r.Match(shebang);
    return m.Success ? m.Result("${commandline}") : ("scalac -deprecation -Yreify-copypaste " + file.FullName);
  } }

  public override bool accept() {
    return file.Extension == ".scala" && compiler != null;
  }

  [Action]
  public virtual ExitCode clean() {
    dir.GetFiles("*.class").ToList().ForEach(file1 => file1.Delete());
    dir.GetFiles("*.log").ToList().ForEach(file1 => file1.Delete());
    return 0;
  }

  [Action]
  public virtual ExitCode rebuild() {
    return clean() && compile();
  }

  [Default, Action]
  public virtual ExitCode compile() {
    var scalaHome = Console.eval("where scalac.bat").FirstOrDefault();
    if (scalaHome == null) {
      println("scalac not found");
      return -1;
    } else {
      scalaHome = Path.GetDirectoryName(scalaHome) + "\\..";
    }

    var status = Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-compiler.jar\" compiler.properties", scalaHome), home: (scalaHome + "\\lib"));
    var compVerFile = scalaHome + "\\lib\\compiler.properties";
    var compVer = null as String;
    try {
      if (!status || !File.Exists(compVerFile)) {
        println("scalac version not found: bad unzip @ scala-compiler.jar");
        return -1;
      } else {
        compVer = File.ReadAllLines(compVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<compVer>.*)$");
          return m.Success ? m.Result("${compVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (compVer == null) {
          println("scalac version not found: bad jar contents @ scala-compiler.jar");
          return -1;
        } else {
          compVer += (" @ " + File.ReadAllLines(compVerFile).First().Substring(1));
        }
      }
    } finally {
      if (File.Exists(compVerFile)) File.Delete(compVerFile);
    }

    status = Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-library.jar\" library.properties", scalaHome), home: (scalaHome + "\\lib"));
    var libVerFile = scalaHome + "\\lib\\library.properties";
    var libVer = null as String;
    try {
      if (!status || !File.Exists(libVerFile)) {
        println("scalac version not found: bad unzip @ scala-library.jar");
        return -1;
      } else {
        libVer = File.ReadAllLines(libVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<libVer>.*)$");
          return m.Success ? m.Result("${libVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (libVer == null) {
          println("scalac version not found: bad jar contents @ scala-library.jar");
          return -1;
        } else {
          libVer += (" @ " + File.ReadAllLines(libVerFile).First().Substring(1));
        }
      }
    } finally {
      if (File.Exists(libVerFile)) File.Delete(libVerFile);
    }

    var parent_key = "Software\\Myke";
    var short_key = ("compile " + file.FullName).Replace("\\", "$slash$");
    var full_key = parent_key + "\\" + short_key;
    var reg = Registry.CurrentUser.OpenSubKey(full_key, true) ?? Registry.CurrentUser.CreateSubKey(full_key);
    var prev_status = reg.GetValue("status") == null ? new ExitCode{value = 0} : new ExitCode{value = (int)reg.GetValue("status")};
    var prev_compiler = reg.GetValue("compiler") as String;
    var prev_scalaHome = reg.GetValue("scalaHome") as String;
    var prev_compVer = reg.GetValue("compVer") as String;
    var prev_libVer = reg.GetValue("libVer") as String;

    if (Config.verbose) {
      println();
      println("===considering caching===");
      println("status: previous value is {0}", prev_status.value);
      println("compiler");
      println("  expected = {0}", prev_compiler);
      println("  actual   = {0}", compiler);
      println("scalaHome");
      println("  expected = {0}", prev_scalaHome);
      println("  actual   = {0}", scalaHome);
      println("compVer");
      println("  expected = {0}", prev_compVer);
      println("  actual   = {0}", compVer);
      println("libVer");
      println("  expected = {0}", prev_libVer);
      println("  actual   = {0}", libVer);
    }

    if (status && prev_compiler == compiler && prev_scalaHome == scalaHome && prev_compVer == compVer && prev_libVer == libVer) {
      var filenames = reg.GetValueNames().Except(new []{"compiler", "scalaHome", "compVer", "libVer", "status"}).ToList();
      var nocache = filenames.Any(filename => {
        var prev_modtime = reg.GetValue(filename) as String;
        filename = filename.Replace("$slash$", "\\");
        var modtime = File.Exists(filename) ? File.GetLastWriteTime(filename).ToString("o") : null;
        if (Config.verbose) {
          println("{0}", filename, prev_modtime, modtime);
          println("  expected = {0}", prev_modtime);
          println("  actual   = {0}", modtime);
        }
        return prev_modtime != modtime;
      });

      if (nocache) {
        if (Config.verbose) {
          println("VERDICT: not going to cache previous compilation results");
          println("=========================");
          println();
        }

        // fall through to the compilation logic
      } else {
        if (Config.verbose) {
          println("VERDICT: caching previous compilation results");
          println("=========================");
          println();
        }

        return status;
      }
    } else {
      if (Config.verbose) {
        println("VERDICT: not going to cache previous compilation results");
        println("=========================");
        println();
      }
    }

    using (var watcher = new FileSystemWatcher(root.FullName)) {
      var files = new List<FileInfo>();
      watcher.Filter = "*.class";
      watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
      watcher.IncludeSubdirectories = true;
      watcher.Created += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.Changed += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.Deleted += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.Renamed += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.EnableRaisingEvents = true;

      status = Console.batch(compiler, home: root);

      var parent_reg = Registry.CurrentUser.OpenSubKey(parent_key, true) ?? Registry.CurrentUser.CreateSubKey(parent_key);
      parent_reg.DeleteSubKey(short_key);
      reg = Registry.CurrentUser.OpenSubKey(full_key, true) ?? Registry.CurrentUser.CreateSubKey(full_key);
      reg.SetValue("compiler", compiler);
      reg.SetValue("scalaHome", scalaHome);
      reg.SetValue("compVer", compVer);
      reg.SetValue("libVer", libVer);
      reg.SetValue("status", status.value);

      files = files.Distinct().ToList();
      files.Where(file1 => file1.Exists).ToList().ForEach(file1 => {
        reg.SetValue(file.FullName.Replace("\\", "$slash$"), file.LastWriteTime.ToString("o"));
        reg.SetValue(file1.FullName.Replace("\\", "$slash$"), file1.LastWriteTime.ToString("o"));
      });

      return status;
    }
  }

  public virtual String inferMainclass() {
    var mains = lines.Select(line => {
      var m = Regex.Match(line, @"object\s+(?<name>.*?)\s+extends\s+App");
      return m.Success ? m.Result("${name}") : null;
    }).Where(main => main != null).ToList();

    return mains.Count() == 1 ? mains.Single() : null;
  }

  public virtual String inferArguments() {
    return lines.Any(line => line.Contains("args")) ? null : "";
  }

  [Action]
  public virtual ExitCode run(Arguments arguments) {
    Func<String> readMainclass = () => inferMainclass() ?? Console.readln(prompt: "Main class", history: String.Format("mainclass {0}", file.FullName));
    Func<String> readArguments = () => inferArguments() ?? Console.readln(prompt: "Run arguments", history: String.Format("run {0}", file.FullName));
    return compile() && Console.interactive("scala " + " " + readMainclass() + " " + (arguments.Count > 0 ? arguments.ToString() : readArguments()), home: root);
  }
}