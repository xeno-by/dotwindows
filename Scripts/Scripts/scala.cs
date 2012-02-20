using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  public static String classpath = @"%PROJECTS%\Kepler\lib\jline.jar;%PROJECTS%\Kepler\lib\fjbg.jar;%PROJECTS%\Kepler\build\locker\classes\compiler;%PROJECTS%\Kepler\build\locker\classes\library";
  //public static String javaopts = "-Dscala.usejavacp=true -Djline.terminal=scala.tools.jline.UnsupportedTerminal";
  public static String javaopts = "-Dscala.usejavacp=true";
  //public static String scalaopts = "-deprecation -Xexperimental -Xmacros -Yreify-copypaste -Yreify-debug -Ymacro-debug -Yshow-trees -uniqid -g:vars";
  public static String scalaopts = "-deprecation -Xexperimental -Xmacros -Yreify-copypaste -Yreify-debug -Ymacro-debug -Ymacro-copypaste -Yshow-trees -uniqid -g:vars";

  public static Dictionary<String, String> profiles = new Dictionary<String, String>();
  static App() {
    profiles.Add("default", @"-cp ""$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.MainGenericRunner $SCALAOPTS$");
    profiles.Add("codelib", @"-cp ""%PROJECTS%\Kepler\test\files\codelib\code.jar;$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.MainGenericRunner $SCALAOPTS$");
    profiles.Add("cont", @"-cp ""$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.MainGenericRunner ""-Xplugin:%PROJECTS%\Kepler\build\locker\classes\continuations.jar"" $SCALAOPTS$");
  }

  public static int Main(String[] args) {
    var matches = profiles.ToDictionary(kvp => kvp.Key, kvp => args.Contains("/" + kvp.Key));
    matches = matches.Where(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var reg = args.Contains("/reg") || args.Contains("/register");
    args = args.Where(arg => arg != "/reg" && arg != "/register" && !args.Contains("/" + arg)).ToArray();
    if (matches.Count() > 1) {
      Console.WriteLine(String.Format("error: multiple profiles {0} specified on the command line contradict each other.", String.Join(", ", matches.Keys.ToArray())));
      return -1;
    }

    var f_profile = new FileInfo("%SCRIPTS_HOME%".Expand() + "\\" + "scala.profile");
    var profile = matches.Count() > 0 ? profiles[matches.Keys.Single()] : profiles["default"];
    if (reg) {
      f_profile.WriteAllText(profile);
      return 0;
    }

    profile = profile.Replace("$CLASSPATH$", classpath);
    profile = profile.Replace("$JAVAOPTS$", javaopts);
    profile = profile.Replace("$SCALAOPTS$", scalaopts);

    var psi = new ProcessStartInfo();
    psi.FileName = @"%JAVA_HOME%\bin\java.exe".Expand();
    psi.Arguments = profile.Expand() + " " + String.Join(" ", args.ToArray());
    psi.UseShellExecute = false;

    var p = Process.Start(psi);
    p.WaitForExit();
    return p.ExitCode;
  }
}


public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}

public static class IO {
  public static String ReadAllText(this FileInfo file) {
    return File.ReadAllText(file.FullName);
  }

  public static List<String> ReadAllLines(this FileInfo file) {
    return File.ReadAllLines(file.FullName).ToList();
  }

  public static void WriteAllText(this FileInfo file, String s) {
    File.WriteAllText(file.FullName, s);
  }

  public static void WriteAllLines(this FileInfo file, IEnumerable<String> lines) {
    File.WriteAllText(file.FullName, String.Join(Environment.NewLine, lines.ToArray()));
  }
}