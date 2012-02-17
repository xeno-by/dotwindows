using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class App {
  public static String miniProfile = "-Xss128m -Xms1024m -Xmx1024m -XX:+UseParallelGC -XX:MaxPermSize=256M";
  public static String maxiProfile = "-Xss128m -Xms2048m -Xmx2048m -XX:+UseParallelGC -XX:MaxPermSize=256M";

  public static int Main(String[] args) {
    var ant_launcher_template = @"%ANT_HOME%\lib\ant-launcher.jar";
    var ant_launcher = ant_launcher_template.Expand();
    if (!File.Exists(ant_launcher)) {
      Console.WriteLine(String.Format("Ant launcher not found at {0} (expanded from {1})", ant_launcher, ant_launcher_template));
      return -1;
    }

    var mini = args.Contains("/mini");
    var maxi = args.Contains("/maxi");
    var reg = args.Contains("/reg") || args.Contains("/register");
    if (mini && maxi) {
      Console.WriteLine("Options /mini and /maxi contradict each other.");
      return -1;
    }

    var f_profile = new FileInfo("%SCRIPTS_HOME%".Expand() + "\\" + "ant.profile");
    if (!f_profile.Exists) f_profile.WriteAllText(maxiProfile);
    var profile = mini ? miniProfile : maxi ? maxiProfile : f_profile.ReadAllText();
    if (reg) {
      f_profile.WriteAllText(profile);
      return 0;
    }

    var ant_args = new List<String>();
    ant_args.Add("-classpath \"" + ant_launcher + "\"");
    ant_args.Add("org.apache.tools.ant.launch.Launcher");
    args.Where(arg => !arg.StartsWith("/")).ToList().ForEach(ant_args.Add);

    var psi = new ProcessStartInfo();
    psi.FileName = @"%JAVA_HOME%\bin\java.exe".Expand();
    psi.Arguments = String.Join(" ", ant_args.ToArray());
    psi.EnvironmentVariables["ANT_OPTS"] = profile;
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