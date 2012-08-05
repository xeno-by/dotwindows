// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /r:LibGit2Sharp.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
  public static String[] profile { get { return File.ReadAllLines("%SCRIPTS_HOME%/scalac.profile".Expand()); } }
  public static String distro { get { return File.ReadAllText("%SCRIPTS_HOME%/scalac.distro".Expand()); } }
  public static String staticJavaopts { get {
    return (profile.ElementAtOrDefault(0) ?? "").Expand(); } }
  public static String staticScalaopts {
    get { return (profile.ElementAtOrDefault(1) ?? "").Expand(); } }

  // public String defaultJavaopts { get { return staticJavaopts + (dir.IsChildOrEquivalentTo("%PROJECTS%\\KeplerUnderRefactoring".Expand()) ? " -Dscala.repl.vids=1 -Dscala.repl.autoruncode=%HOME%/.scala_autorun -Dscala.repl.maxprintstring=0" : "");; } }
  public String defaultJavaopts { get { return staticJavaopts; } }
  public String defaultScalaopts { get { return staticScalaopts; } }

  public Scala(FileInfo file, Arguments arguments) : base(file) { init(arguments); warnIfJava7(); }
  public Scala(DirectoryInfo dir, Arguments arguments) : base(dir) { init(arguments); warnIfJava7(); }
  private void warnIfJava7() {
    var javaVer = Console.eval("java -version")[0];
    if (javaVer.Contains("1.7")) println("[" + javaVer + "]");
  }

  private List<FileInfo> sources;
  private List<String> flags;
  private void init(Arguments arguments) {
    var head = ((FileSystemInfo)file ?? dir).FullName;
    if (head == Path.GetFullPath(".")) head = ".";
    else {
      // head = ((FileSystemInfo)file ?? dir).Name;
      if (head.StartsWith(Path.GetFullPath("."))) head = head.Substring(Path.GetFullPath(".").Length);
      if (head.StartsWith("/") || head.StartsWith("\\")) head = head.Substring(1);
    }

    arguments = new Arguments((arguments ?? new Arguments()).Concat(new List<String>{head}).ToList());
    var combo = arguments.SelectMany<String, Object>(argument => {
      if (File.Exists(argument)) {
        var file1 = new FileInfo(argument);
        return new List<Object>{file1};
      } else if (Directory.Exists(argument)) {
        var dir1 = new DirectoryInfo(argument);
        var scalas = dir1.GetFiles("*.scala", SearchOption.TopDirectoryOnly).ToList();
        var javas = dir1.GetFiles("*.java", SearchOption.TopDirectoryOnly).ToList();
        return Enumerable.Concat(scalas, javas).ToList();
      } else {
        return new List<Object>{argument};
      }
    }).ToList();
    sources = combo.OfType<FileInfo>().Where(fi => fi.Extension == ".scala" || fi.Extension == ".java").ToList();
    flags = combo.OfType<String>().ToList();

    env["ResultFileRegex"] = "([:.a-z_A-Z0-9\\\\/-]+[.]scala):([0-9]+)";
  }

  public override bool accept() {
    var action = Config.action;
    if (action == "diff-vanilla-and-alt") return true;
    if (action.EndsWith("-alt")) action = action.Substring(0, action.Length - 4);
    return (action == "compile" && sources.Count() > 0 && compiler != null) || action == "run" || action == "repl";
  }

  public virtual String compiler { get {
    Func<FileInfo, String> inferCompiler = fi => {
      var lines = fi.Exists ? File.ReadAllLines(fi.FullName) : new String[]{};
      var shebang = lines.ElementAtOrDefault(0) ?? "";
      var r = new Regex("^\\s*//\\s*build\\s+this\\s+with\\s+\"(?<commandline>.*)\"\\s*$");
      var m = r.Match(shebang);
      return m.Success ? m.Result("${commandline}") : null;
    };

    var inferreds = sources.Select(inferCompiler).Where(s => s != null).Distinct().ToList();
    if (inferreds.Count > 1) { println("scalac: ambiguous compiler shebangs"); return null; }
    if (inferreds.Count > 0 && sources.Count > 1) { println("scalac: cannot use compiler shebangs with multiple sources"); return null; }
    if (inferreds.Count > 0 && flags.Count > 1) { println("scalac: cannot use compiler shebangs with non-empty flags"); return null; }
    var inferred = inferreds.SingleOrDefault();

    Func<String> defaultCompiler = () => {
      var indices = sources.Select(fi => {
        var iof = fi.Name.LastIndexOf("_");
        if (iof == -1) return (int?) null;
        var suffix = Path.GetFileNameWithoutExtension(fi.Name.Substring(iof + 1));
        int parsed;
        if (int.TryParse(suffix, out parsed)) return (int?)parsed;
        else return (int?)null;
      }).ToList();

      if (indices.Where(i => i != null).Distinct().Count() == indices.Count()) {
        var ordered = indices.Zip(sources, (index, source) => new KeyValuePair<int, FileInfo>(index.Value, source)).OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        var compilers = ordered.Select(fi => "scalac " + fi.Name.ShellEscape() + (flags.Count() == 0 ? "" : " " + String.Join(" ", flags.ToArray()))).ToList();
        return String.Join(Environment.NewLine, compilers.ToArray());
      } else {
        return "scalac " + String.Join(" ", flags.Concat(sources.Select(file => file.Name.ShellEscape())).ToArray());
      }
    };

    return inferred ?? defaultCompiler();
  } }

  public virtual String buildCompilerInvocation(String command, bool alt) {
    command = command.Expand();

    var magicPrefix = "scalac ";
    if (!command.StartsWith(magicPrefix)) { println("scalac: compiler command must start from \"" + magicPrefix + "\""); return null; }
    command = command.Substring(magicPrefix.Length);

    var javaParts = command.Split(' ').Where(part => part.StartsWith("-D")).ToList();
    var javaNoDefaults = javaParts.Contains("-Dnodefault") || javaParts.Contains("-Dnodefaults");
    javaParts = javaParts.Where(part => part != "-Dnodefault" && part != "-Dnodefaults").ToList();
    if (!javaNoDefaults) javaParts = Enumerable.Concat(defaultJavaopts.Split(' '), javaParts).ToList();
    javaParts.ToList().ForEach(part => {
      if (part.StartsWith("-Dno")) {
        var negation = "-D" + part.Substring(4);
        javaParts.Remove(part);
        javaParts.Remove(negation);
      }
    });
    var javaOpts = String.Join(" ", javaParts.ToArray());
    if (Config.sublime) javaOpts += " -Djline.terminal=scala.tools.jline.UnsupportedTerminal";

    var scalaParts = command.Split(' ').Where(part => !part.StartsWith("-D")).ToList();
    var scalaNoDefaults = scalaParts.Contains("-nodefault") || scalaParts.Contains("-nodefaults");
    scalaParts = scalaParts.Where(part => part != "-nodefault" && part != "-nodefaults").ToList();
    if (!scalaNoDefaults) scalaParts = Enumerable.Concat(defaultScalaopts.Split(' '), scalaParts).ToList();
    scalaParts.ToList().ForEach(part => {
      if (part.StartsWith("-no")) {
        var negation = "-" + part.Substring(3);
        scalaParts.Remove(part);
        scalaParts.Remove(negation);
      }
    });
    var scalaOpts = String.Join(" ", scalaParts.ToArray());

    var f_classpathConfig = new FileInfo("%SCRIPTS_HOME%/scalac.classpath".Expand());
    var useBootClasspath = f_classpathConfig.Exists && File.ReadAllText(f_classpathConfig.FullName) == "boot";
    var nobootTemplate = @"%JAVA_HOME%\bin\java.exe -cp ""$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.Main $SCALAOPTS$";
    var bootTemplate = @"%JAVA_HOME%\bin\java.exe -Xbootclasspath/a:""$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.Main $SCALAOPTS$";

    command = useBootClasspath ? bootTemplate : nobootTemplate;
    command = command.Replace("$CLASSPATH$", inferScalaClasspath(alt));
    command = command.Replace("$JAVAOPTS$", javaOpts);
    command = command.Replace("$SCALAOPTS$", scalaOpts);

    // println(command);
    return command.Expand();
  }

  public virtual String buildRunnerInvocation(String mainClass, String arguments, bool alt) {
    var invocation = buildCompilerInvocation("scalac " + " " + mainClass + " " + arguments, alt);
    return invocation.Replace(" scala.tools.nsc.Main ", " scala.tools.nsc.MainGenericRunner ");
  }

  public virtual String buildReplInvocation(bool alt) {
    var invocation = buildCompilerInvocation("scalac " + String.Join(" ", flags.ToArray()), alt: alt);
    return invocation.Replace(" scala.tools.nsc.Main ", " scala.tools.nsc.MainGenericRunner ");
  }

  public virtual String inferScalaSourcesRoot(bool alt) {
    alt = alt || distro == "alt";
    if (alt) {
      return new DirectoryInfo(@"%PROJECTS%\Scala".Expand()).FullName;
    } else {
      var target = dir;
      DirectoryInfo sourcesRoot = null;
      while (target.FullName != target.Root.FullName) {
        var token = target.GetFiles("pull-binary-libs.sh").FirstOrDefault();
        if (token != null) {
          sourcesRoot = target;
          break;
        }
        target = target.Parent;
      }

      sourcesRoot = sourcesRoot ?? new DirectoryInfo(@"%PROJECTS%\Kepler".Expand());
      return sourcesRoot.FullName;
    }
  }

  public virtual String inferScalaHome(bool alt) {
    var sourcesRoot = inferScalaSourcesRoot(alt);
    return @"%ROOT%\build\locker\classes".Replace("%ROOT%", sourcesRoot);
  }

  public virtual String inferScalaClasspath(bool alt) {
    var sourcesRoot = inferScalaSourcesRoot(alt);
    return @"%ROOT%\test\files\codelib\code.jar;%ROOT%\lib\jline.jar;%ROOT%\lib\fjbg.jar;%ROOT%\build\locker\classes\compiler;%ROOT%\build\asm\classes;%ROOT%\build\libs\forkjoin.jar;%ROOT%\build\locker\classes\reflect;%ROOT%\build\locker\classes\library".Replace("%ROOT%", sourcesRoot);
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
    return compileImpl(alt: false);
  }

  [Action]
  public virtual ExitCode compileAlt() {
    return compileImpl(alt: true);
  }

  public virtual ExitCode compileImpl(bool alt) {
    if (!accept()) { println("scalac: don't know how to compile the stuff you asked"); return -1; }
    ExitCode status = null;

    var scalaHome = inferScalaHome(alt);
    if (scalaHome == null) { println("scalac: don't know how to infer my home"); return -1; }

    var classes = scalaHome.EndsWith("classes");
    classes = classes || scalaHome.EndsWith("classes\\");

    status = classes ? 0 : Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-compiler.jar\" compiler.properties", scalaHome), home: (scalaHome + "\\lib"));
    var compVerFile = classes ? (scalaHome + "\\compiler\\compiler.properties") : (scalaHome + "\\lib\\compiler.properties");
    var compVer = null as String;
    try {
      if (!status || !File.Exists(compVerFile)) {
        println("scalac: bad unzip @ scala-compiler.jar");
        return -1;
      } else {
        compVer = File.ReadAllLines(compVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<compVer>.*)$");
          return m.Success ? m.Result("${compVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (compVer == null) {
          println("scalac: bad jar contents @ scala-compiler.jar");
          return -1;
        } else {
          compVer += (" @ " + File.ReadAllLines(compVerFile).First().Substring(1));
        }
      }
    } finally {
      if (!classes && File.Exists(compVerFile)) File.Delete(compVerFile);
    }

    status = classes ? 0 : Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-reflect.jar\" reflect.properties", scalaHome), home: (scalaHome + "\\lib"));
    var reflVerFile = classes ? (scalaHome + "\\reflect\\reflect.properties") : (scalaHome + "\\lib\\reflect.properties");
    var reflVer = null as String;
    try {
      if (!status || !File.Exists(reflVerFile)) {
        // println("scalac: bad unzip @ scala-reflect.jar");
        // return -1;
        reflVer = "n/a";
      } else {
        reflVer = File.ReadAllLines(reflVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<reflVer>.*)$");
          return m.Success ? m.Result("${reflVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (reflVer == null) {
          println("scalac: bad jar contents @ scala-reflect.jar");
          return -1;
        } else {
          reflVer += (" @ " + File.ReadAllLines(reflVerFile).First().Substring(1));
        }
      }
    } finally {
      if (!classes && File.Exists(reflVerFile)) File.Delete(reflVerFile);
    }

    status = classes ? 0 : Console.batch(String.Format("unzip -o -q \"{0}\\lib\\scala-library.jar\" library.properties", scalaHome), home: (scalaHome + "\\lib"));
    var libVerFile = classes ? (scalaHome + "\\library\\library.properties") : (scalaHome + "\\lib\\library.properties");
    var libVer = null as String;
    try {
      if (!status || !File.Exists(libVerFile)) {
        println("scalac: bad unzip @ scala-library.jar");
        return -1;
      } else {
        libVer = File.ReadAllLines(libVerFile).Select(line => {
          var m = Regex.Match(line, @"^version.number=(?<libVer>.*)$");
          return m.Success ? m.Result("${libVer}") : null;
        }).Where(ver => ver != null).FirstOrDefault();

        if (libVer == null) {
          println("scalac: bad jar contents @ scala-library.jar");
          return -1;
        } else {
          libVer += (" @ " + File.ReadAllLines(libVerFile).First().Substring(1));
        }
      }
    } finally {
      if (!classes && File.Exists(libVerFile)) File.Delete(libVerFile);
    }

    var parent_key = "Software\\Myke";
    var short_key = (compiler + " @ " + dir.FullName).Replace("\\", "$slash$");
    var full_key = parent_key + "\\" + short_key;
    var reg = Registry.CurrentUser.OpenSubKey(full_key, true) ?? Registry.CurrentUser.CreateSubKey(full_key);
    var prev_status = reg.GetValue("status") == null ? new ExitCode{value = 0} : new ExitCode{value = (int)reg.GetValue("status")};
    var prev_compiler = reg.GetValue("compiler") as String;
    var prev_scalaHome = reg.GetValue("scalaHome") as String;
    var prev_compVer = reg.GetValue("compVer") as String;
    var prev_reflVer = reg.GetValue("reflVer") as String;
    var prev_libVer = reg.GetValue("libVer") as String;

    if (Config.verbose) {
      println();
      println("===considering caching===");
      println("status: previous value is {0}", prev_status.value);

      if (prev_status) {
        println("compiler");
        println("  expected = {0}", prev_compiler);
        println("  actual   = {0}", compiler);
        println("scalaHome");
        println("  expected = {0}", prev_scalaHome);
        println("  actual   = {0}", scalaHome);
        println("compVer");
        println("  expected = {0}", prev_compVer);
        println("  actual   = {0}", compVer);
        println("reflVer");
        println("  expected = {0}", prev_reflVer);
        println("  actual   = {0}", reflVer);
        println("libVer");
        println("  expected = {0}", prev_libVer);
        println("  actual   = {0}", libVer);
      }
    }

    if (prev_status && prev_compiler == compiler && prev_scalaHome == scalaHome && prev_compVer == compVer && prev_reflVer == reflVer && prev_libVer == libVer) {
      var filenames = reg.GetValueNames().Except(new []{"compiler", "scalaHome", "compVer", "reflVer", "libVer", "status"}).ToList();
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
      var files = new List<FileInfo>(sources);
      watcher.Filter = "*.class";
      watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
      watcher.IncludeSubdirectories = true;
      watcher.Created += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.Changed += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.Deleted += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.Renamed += (o, e) => files.Add(new FileInfo(e.FullPath));
      watcher.EnableRaisingEvents = true;

      if (alt || distro == "alt") println("[compiling with scala-alt]");
      var compilers = compiler.Split(new []{Environment.NewLine}, StringSplitOptions.None).ToList();
      status = compilers.Aggregate((ExitCode)0, (curr, compiler1) => {
        if (!curr) return curr;
        var invocation = buildCompilerInvocation(compiler1, alt);
        if (invocation != null && invocation.Contains("-Xprompt")) return Console.interactive(invocation, home: dir);
        return Console.batch(invocation, home: dir);
      });

      var parent_reg = Registry.CurrentUser.OpenSubKey(parent_key, true) ?? Registry.CurrentUser.CreateSubKey(parent_key);
      parent_reg.DeleteSubKey(short_key);
      reg = Registry.CurrentUser.OpenSubKey(full_key, true) ?? Registry.CurrentUser.CreateSubKey(full_key);
      reg.SetValue("compiler", compiler);
      reg.SetValue("scalaHome", scalaHome);
      reg.SetValue("compVer", compVer);
      reg.SetValue("reflVer", reflVer);
      reg.SetValue("libVer", libVer);
      reg.SetValue("status", status.value);

      files = files.Distinct().ToList();
      files.Where(file1 => file1.Exists).ToList().ForEach(file1 => {
        reg.SetValue(file1.FullName.Replace("\\", "$slash$"), file1.LastWriteTime.ToString("o"));
        var s_symlink = GetSymlinkTarget(file1.FullName);
        var symlink = s_symlink == null ? null : new FileInfo(s_symlink);
        if (symlink != null) reg.SetValue(symlink.FullName.Replace("\\", "$slash$"), symlink.LastWriteTime.ToString("o"));
      });

      return status;
    }
  }

  private static String GetSymlinkTarget(String path) {
    using (var process = Process.Start(new ProcessStartInfo("junction", String.Format("\"{0}\"", path)){UseShellExecute = false, RedirectStandardOutput = true})) {
      using (var reader = process.StandardOutput) {
        var lines = reader.ReadToEnd().Split(new []{Environment.NewLine}, StringSplitOptions.None);
        if (!((lines.Length > 5) && lines[5].Contains("SYMBOLIC LINK"))) return null;
        var symlink = lines[6];
        int iof = symlink.IndexOf(": ");
        return symlink.Substring(iof + 2);
      }
    }
  }

  [Action, MenuItem(hotkey = "d", description = "Diff vanilla and alt", priority = 70)]
  public virtual ExitCode diffVanillaAndAlt() {
    var mods = flags.Count != 0 ? new Arguments(flags).ToString() : "-Xprint:typer,cleanup";
    var status = clean();
    var uid = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
    status = status && println("Building with alt...");
    status = status && Console.batch(String.Format("scalacalt {0} {1} > %TMP%/alt.{2}.log", mods, file != null ? file.FullName : dir.FullName, uid));
    status = clean();
    status = status && println("Building with vanilla...");
    status = status && Console.batch(String.Format("scalac {0} {1} > %TMP%/vanilla.{2}.log", mods, file != null ? file.FullName : dir.FullName, uid));
    status = clean();
    var diffcmd = String.Format("mydiff %TMP%/alt.{0}.log %TMP%/vanilla.{0}.log".Expand(), uid);
    status = status && println("Running " + diffcmd);
    return status && Console.ui(diffcmd);
  }

  public virtual String inferMainclass() {
    var mains = sources.SelectMany(fi => {
      var lines = fi.Exists ? File.ReadAllLines(fi.FullName) : new String[]{};
      return lines.Select(line => {
        var m = Regex.Match(line, @"object\s+(?<name>.*?)\s+extends\s+App");
        return m.Success ? m.Result("${name}") : null;
      }).Where(main => main != null).ToList();
    }).ToList();

    return mains.Count() == 1 ? mains.Single() : null;
  }

  public virtual String inferArguments() {
    var needsArgs = sources.SelectMany(fi => fi.Exists ? File.ReadAllLines(fi.FullName) : new String[]{}).Any(line => Regex.Match(line, @"\Bargs\B").Success && !line.Contains("args: ") && !line.Contains("args:"));
    return needsArgs ? null : "";
  }

  [Action, Meaningful]
  public virtual ExitCode run() {
    return runImpl(alt: false);
  }

  [Action, Meaningful]
  public virtual ExitCode runAlt() {
    return runImpl(alt: true);
  }

  public virtual ExitCode runImpl(bool alt) {
    if (alt || distro == "alt") println("[running with scala-alt]");
    Func<String> readMainclass = () => inferMainclass() ?? Console.readln(prompt: "Main class", history: String.Format("mainclass {0}", compiler));
    Func<String> readArguments = () => inferArguments() ?? Console.readln(prompt: "Run arguments", history: String.Format("run {0}", compiler));
//    return compileImpl(alt) && Console.interactive(buildRunnerInvocation(readMainclass(), readArguments()), home: dir);
    return compileImpl(alt) && Console.batch(buildRunnerInvocation(readMainclass(), readArguments(), alt), home: dir);
  }

  [Action, DontTrace]
  public virtual ExitCode repl() {
    return replImpl(alt: false);
  }

  [Action, DontTrace]
  public virtual ExitCode replAlt() {
    return replImpl(alt: true);
  }

  public virtual ExitCode replImpl(bool alt) {
    if (alt || distro == "alt") println("[repling with scala-alt]");
    return Console.interactive(buildReplInvocation(alt), home: inferScalaHome(alt));
  }
}