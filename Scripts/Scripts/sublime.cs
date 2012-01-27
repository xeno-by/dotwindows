// build this with "csc /r:System.Windows.Forms.dll /t:winexe sublime.cs"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var sublime = @"C:\Program Files\Sublime Text 2\sublime_text.exe";
    var home = new DirectoryInfo(@"%APPDATA%\Sublime Text 2".Expand());

    var projects = new DirectoryInfo(home.FullName + @"\Projects");
    var default_prj = null as String;
    if (projects.Exists && projects.GetFiles("*.is.default").Length == 1) {
      var default_file = projects.GetFiles("*.is.default")[0];
      default_prj = Path.GetFileNameWithoutExtension(default_file.FullName);
      default_prj = Path.GetFileNameWithoutExtension(default_prj);
      if (args.Length == 0) args = new []{default_prj};
    }

    if (args.Length > 0) {
      var file0 = new FileInfo(args[0]);
      var dir0 = new DirectoryInfo(args[0]);
      if (file0.Exists) dir0 = file0.Directory;

      if (file0.Exists || dir0.Exists) {
        var idx = 0;
        Func<FileInfo> project = () => new FileInfo(projects + "\\" + dir0.Name + (idx == 0 ? "" : idx.ToString()) + ".sublime-project");
        while (project().Exists) idx++;


        var lines = new List<String>();
        lines.Add("{");
        lines.Add("  \"folders\":");
        lines.Add("  [");
        for (var i = 0; i < args.Length; ++i) {
          var file = new FileInfo(args[i]);
          var dir = new DirectoryInfo(args[i]);
          if (file.Exists) dir = file.Directory;

          var dir_as_sublimepath = dir.FullName;
          dir_as_sublimepath = dir_as_sublimepath.Replace("\\", "/");
          dir_as_sublimepath = dir_as_sublimepath.Replace(":/", "/");
          dir_as_sublimepath = "/" + dir_as_sublimepath;

          lines.Add("    {");
          lines.Add("      \"path\": \"" + dir_as_sublimepath + "\"");
          lines.Add("    }");
          if (i != args.Length - 1) lines[lines.Count - 1] = lines[lines.Count - 1] + ",";
        }
        lines.Add("  ]");
        lines.Add("}");
        project().WriteAllText(String.Join(Environment.NewLine, lines.ToArray()));

        args = new []{"--project", "\"" + project().FullName + "\""}.Concat(args).ToArray();
      } else {
        var project = new FileInfo(projects + "\\" + args[0] + ".sublime-project");
        if (!project.Exists) {
          MessageBox.Show(String.Format("Project \"{0}\" not found. \r\n\r\n" +
                                        "Could not find project file {1}.", args[0], project),
                                        "Sublime Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        } else {
          if (args.Length == 2 && (args[1].ToLower() == "/reg" || args[1].ToLower() == "/register")) {
            projects.GetFiles("*.is.default").ToList().ForEach(file => file.Delete());
            var default_file = new FileInfo(Path.ChangeExtension(project.FullName, "is.default"));
            default_file.WriteAllText("");
            args = args.Take(1).Concat(args.Skip(2)).ToArray();
          }

          if (args.Length != 1) return;
          args = new []{"--project", "\"" + project.FullName + "\""};
        }
      }
    }

    if (args.Length == 0) {
      MessageBox.Show("No files, directories or projects specified. \r\n\r\nTo make sublime run without any arguments " +
                      "open a default project, first register the project via \"sublime <project> /reg\".",
                      "Sublime Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return;
    }

    Process.Start(sublime, String.Join(" ", args));
  }
}


public static class Env {
  public static String Expand(this String s) {
    return new Regex("%(?<envvar>.*?)%").Replace(s, m => Environment.GetEnvironmentVariable(m.Result("${envvar}")));
  }
}

public static class IO {
  public static void WriteAllText(this FileInfo file, String s) {
    File.WriteAllText(file.FullName, s);
  }
}