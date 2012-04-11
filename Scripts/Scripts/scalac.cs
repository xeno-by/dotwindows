using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  public static String classpath = @"%PROJECTS%\Kepler\lib\fjbg.jar;%PROJECTS%\Kepler\build\locker\classes\compiler;%PROJECTS%\Kepler\build\locker\classes\library";
  public static String upstreamclasspath = @"%PROJECTS%\ScalaUpstream\lib\fjbg.jar;%PROJECTS%\ScalaUpstream\build\locker\classes\compiler;%PROJECTS%\ScalaUpstream\build\locker\classes\library";
  public static String javaopts = "-Dscala.usejavacp=true";
  public static String scalaopts = "-deprecation -unchecked -Xexperimental -Xmacros -Xlog-implicits -Ymacro-copypaste -Yshow-trees-compact -Yshow-trees-stringified -g:vars";
  public static String upstreamscalaopts = "-deprecation -unchecked -Xexperimental -g:vars";

  public static Dictionary<String, String> profiles = new Dictionary<String, String>();
  static App() {
    profiles.Add("default", @"-cp ""$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.Main $SCALAOPTS$");
    profiles.Add("codelib", @"-cp ""%PROJECTS%\Kepler\test\files\codelib\code.jar;$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.Main $SCALAOPTS$");
    profiles.Add("cont", @"-cp ""$CLASSPATH$"" $JAVAOPTS$ scala.tools.nsc.Main ""-Xplugin:%PROJECTS%\Kepler\build\locker\classes\continuations.jar"" $SCALAOPTS$");
  }

  public static int Main(String[] args) {
    var matches = profiles.ToDictionary(kvp => kvp.Key, kvp => args.Contains("/" + kvp.Key));
    matches = matches.Where(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var reg = args.Contains("/reg") || args.Contains("/register");
    var upstream = args.Contains("/upstream");
    var noupstream = args.Contains("/noupstream");
    args = args.Where(arg => arg != "/reg" && arg != "/register" && arg != "/upstream" && arg != "/noupstream" && !args.Contains("/" + arg)).ToArray();
    if (matches.Count() > 1) {
      Console.WriteLine(String.Format("error: multiple profiles {0} specified on the command line contradict each other.", String.Join(", ", matches.Keys.ToArray())));
      return -1;
    }

    var f_profile = new FileInfo("%SCRIPTS_HOME%".Expand() + "\\" + "scalac.profile");
    var profile = matches.Count() > 0 ? profiles[matches.Keys.Single()] : profiles["default"];
    if (reg) {
      f_profile.WriteAllText(profile);
      return 0;
    }

    if ((upstream || Environment.CurrentDirectory.Contains(@"%PROJECTS%\ScalaUpstream".Expand())) && !noupstream) {
      profile = profile.Replace("$CLASSPATH$", upstreamclasspath);
      profile = profile.Replace("$JAVAOPTS$", javaopts);
      profile = profile.Replace("$SCALAOPTS$", upstreamscalaopts);
    } else {
      profile = profile.Replace("$CLASSPATH$", classpath);
      profile = profile.Replace("$JAVAOPTS$", javaopts);
      profile = profile.Replace("$SCALAOPTS$", scalaopts);
    }

    if (args.Contains("-nouniqid")) {
      args = args.Where(arg => arg != "-uniqid" && arg != "-nouniqid").ToArray();
      profile = profile.Replace("-uniqid", "");
    }

    if (args.Contains("-Ynoshow-trees")) {
      args = args.Where(arg => arg != "-Yshow-trees" && arg != "-Ynoshow-trees").ToArray();
      profile = profile.Replace("-Yshow-trees", "");
    }

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