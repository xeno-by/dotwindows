// build this with "csc /t:winexe filecop.cs"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

public class App {
  private static String path = ".";
  private static String mask = "*";
  private static String mode = "filecop";

  [STAThread]
  public static int Main(String[] args) {
    var arg_path = args.ElementAtOrDefault(0);
    if (arg_path != null) path = arg_path;

    var arg_mask = args.ElementAtOrDefault(1);
    if (arg_mask != null) mask = arg_mask;

    if (path.StartsWith("\"") && path.EndsWith("\"")) path = path.Substring(1, path.Length - 2);
    if (File.Exists(path)) { ValidateFile(new FileInfo(path)); }
    else if (Directory.Exists(path)) { ValidateDirectory(new DirectoryInfo(path)); }
    else { Console.WriteLine(String.Format("error: file or directory {0} does not exist", path)); return 1; }
    return 0;
  }

  private static void ValidateDirectory(DirectoryInfo d) {
    foreach (var f in d.EnumerateFiles(mask, SearchOption.AllDirectories))
      ValidateFile(f);
  }

  private static void ValidateFile(FileInfo f) {
    try {
      var offenses = GetOffenses(f);
      if (offenses.Count > 0) FixOffenses(f, offenses);
    } catch (PathTooLongException) {
      // ignore and continue traversal
    }
  }

  private static List<String> GetOffenses(FileInfo f) {
    var offenses = new List<String>();
    GetOffensesImpl(f, offenses);
    return offenses;
  }

  private static void GetOffensesImpl(FileInfo f, List<String> offenses) {
    if (f.FullName.Contains("\\.settings\\")) return;
    if (f.FullName.Contains("\\.externalToolBuilders\\")) return;
    if (f.FullName.Contains("\\target\\")) return;
    if (f.FullName.Contains("\\build\\")) return;
    if (f.FullName.Contains("\\dists\\")) return;
    if (f.FullName.Contains("\\docs\\")) return;
    if (f.FullName.Contains("\\doc\\")) return;
    if (f.FullName.Contains("\\images\\")) return;
    if (f.FullName.Contains("\\.git\\")) return;
    if (f.FullName.Contains("\\.svn\\")) return;
    if (f.FullName.Contains("\\.hg\\")) return;
    if (f.FullName.Contains("\\test\\") && !f.FullName.Contains("\\test\\kepler\\")) return;
    if (f.Extension == ".jar" || f.Extension == ".class" || f.Extension == ".cache") return;

    var bytes = File.ReadAllBytes(f.FullName);
    for (var i = 0; i < bytes.Length - 1; ++i) {
      if (bytes[i] == 0x09) {
        if (f.Extension == ".scala") {
          if (!offenses.Contains("TAB")) offenses.Add("TAB");
        }
      }

      if (bytes[i] == 0x0D && bytes[i+1] == 0x0A) {
        if (f.Extension == ".bat" || f.Extension == ".cmd") continue;
        if (!offenses.Contains("EOLN")) offenses.Add("EOLN");
      }
    }
  }

  private static void FixOffenses(FileInfo f, List<String> offenses) {
    if (mode == "filecop") {
      Console.WriteLine(String.Format("{0}: {1}", f.FullName, String.Join(", ", offenses.ToArray())));
    } else if (mode == "filefix") {
      try {
        var lines = File.ReadLines(f.FullName);

        f.Delete();
        using (var s = f.OpenWrite()) {
          using (var w = new StreamWriter(s)) {
            foreach (var line in lines) {
              line.Replace("\t", "  ");
              w.Write(line);
              s.WriteByte((byte)0x0A);
            }
          }
        }
      } catch (Exception) {
        Console.WriteLine(" [FAILED]");
        Console.WriteLine();
        throw;
      }

      Console.WriteLine(" [OK]");
    } else {
      throw new NotSupportedException("unknown mode " + mode);
    }
  }
}