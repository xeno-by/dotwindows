// build this with "csc /t:exe /out:movetraits.exe /debug+ movetraits.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class App {
  public static void Main(String[] args) {
    // https://github.com/scalamacros/kepler/blob/3afb125bc500658e6fc3ec3079c509a6e59be866/src/library/scala/reflect/base/Trees.scala#L1
    var lines = File.ReadAllLines(@"C:\Projects\KeplerUnderRefactoring\src\library\scala\reflect\base\Trees.scala");
    var buckets = new List<List<String>>();
    var remnants = new List<String>();

    var running = false;
    var bucket = new List<String>();
    for (var i = 0; i < lines.Count(); ++i) {
      var line = lines[i];

      if (line.Trim().StartsWith("trait ") && !line.Trim().StartsWith("trait Trees ") && !line.Trim().StartsWith("trait TreeBase ")) {
        if (running) throw new Exception("boom!");
        running = true;
        bucket = new List<String>();
        bucket.Insert(0, line.Replace("Base", "Api"));
        bucket.Insert(0, lines[i - 1]);
        bucket.Insert(0, lines[i - 2]);
        remnants.RemoveRange(remnants.Count() - 2, 2);
        for (var j = remnants.Count() - 1; j >= 0; --j) {
          var rline = remnants[j];
          if (rline.Trim().StartsWith("type ") && rline.Trim() != "type Tree >: Null <: TreeBase") {
            var pprline = "  override " + rline.Trim().Replace("Base", "Api");
            var liof = rline.LastIndexOf(" with ") == -1 ? rline.LastIndexOf(" extends ") : rline.LastIndexOf(" with ");
            if (liof == -1) {
              liof = rline.LastIndexOf(" <: ");
              if (liof == -1) {
                Console.WriteLine(line);
                Console.WriteLine(rline);
              }
              var ppline = rline.Substring(0, liof) + " <: AnyRef";
              remnants[j] = ppline;
              bucket.Insert(0, pprline);
              bucket.Insert(0, "  /** .. */");
            } else {
              var ppline = rline.Substring(0, liof);
              remnants[j] = ppline;
              bucket.Insert(0, pprline);
              bucket.Insert(0, "  /** .. */");
            }
            break;
          }
        }
        continue;
      }

      if (running) {
        bucket.Add(line);
        if (line.Trim() == "}") {
          running = false;
          buckets.Add(bucket);
        }
      } else {
        remnants.Add(line);
      }
    }

    File.WriteAllText(@"d:\1.scala", String.Join("\n", remnants.ToArray()));
    File.WriteAllText(@"d:\2.scala", String.Join("\n\n", buckets.Select(bucket1 => String.Join("\n", bucket1.Select(bb => bb.Replace("The base API", "The API")).ToArray())).ToArray()));
  }
}