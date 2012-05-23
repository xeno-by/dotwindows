using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class App {
  public static void Main(String[] args) {
    // var root = new DirectoryInfo(@"c:\Projects\KeplerUnderRefactoring\test\files");
    var root = new DirectoryInfo(@"c:\Projects\KeplerUnderRefactoring\test\pending");
    var files = root.GetFiles("*.scala", SearchOption.AllDirectories).ToList();
    files.ForEach(file => {
      var lines = File.ReadAllLines(file.FullName).ToList();
      var orig_lines = File.ReadAllLines(file.FullName).ToList();

      if (lines.Count() > 0) {
        var hasImports = lines.First().StartsWith("import ");
        var initDone = false;
        Action init = () => {
          if (initDone) return;
          initDone = true;
          if (!hasImports) lines.Insert(0, "");
        };

        // var usesClassTag = lines.Where(line => line.Trim().Contains("ClassTag")).Count() > 0;
        // if (usesClassTag) {
        //   init();
        //   lines.Insert(0, "import scala.reflect.{ClassTag, classTag}");
        // }

        // var usesArrayTag = lines.Where(line => line.Trim().Contains("ArrayTag")).Count() > 0;
        // if (usesArrayTag) {
        //   init();
        //   lines.Insert(0, "import scala.reflect.{ArrayTag, arrayTag}");
        // }

        // var usesRu = lines.Where(line => line.Trim().Contains("typeTag") ||
        //                                  line.Trim().Contains("TypeTag") ||
        //                                  line.Trim().Contains("concreteTypeTag") ||
        //                                  line.Trim().Contains("ConcreteTypeTag")).Count() > 0;
        // if (usesRu) {
        //   init();
        //   lines.Insert(0, "import scala.reflect.runtime.universe._");
        // }

        // var modded = false;
        // lines = lines.Select(line => {
        //   if (line == "import scala.reflect.mirror._") {
        //     modded = true;
        //     if (!lines.Contains("import scala.reflect.runtime.universe._")) return "import scala.reflect.runtime.universe._";
        //     else return null;
        //   } else {
        //     return line;
        //   }
        // }).Where(line => line != null).ToList();

        // var modded = false;
        // lines = lines.Select(line => {
        //   if (line.Trim() == "import c.mirror._") {
        //     modded = true;
        //     var ipad = line.IndexOf("import");
        //     var pad = new String(' ', ipad);
        //     if (lines.Where(line1 => line1.Trim() == "import c.universe._").Count() == 0) return pad + "import c.universe._";
        //     else return null;
        //   } else {
        //     return line;
        //   }
        // }).Where(line => line != null).ToList();

        // var modded = false;
        // var usesRuntimeEval = lines.Where(line => line.Trim().Contains(".runtimeEval")).Count() > 0;
        // if (usesRuntimeEval) {
        //   var iof = lines.IndexOf("import scala.reflect.runtime.universe._");
        //   if (iof == -1) {
        //     Console.WriteLine("*" + file.FullName);
        //     return;
        //   }
        //   modded = true;
        //   lines.Insert(iof + 1, "import scala.tools.reflect.RuntimeEval");
        // }

        var modded = false;
        var usesMkToolBox = lines.Where(line => line.Trim().Contains("mkToolBox()")).Count() > 0;
        if (usesMkToolBox) {
          var iof = lines.IndexOf("import scala.reflect.runtime.universe._");
          if (iof == -1) {
            Console.WriteLine("*" + file.FullName);
            return;
          }
          modded = true;
          lines.Insert(iof + 1, "import scala.tools.reflect.{universe => ru}");
          lines = lines.Select(line => line.Replace("mkToolBox()", "ru.mkToolBox()")).ToList();
        }

        // if (lines.Count() != orig_lines.Count()) {
        if (modded || lines.Count() != orig_lines.Count()) {
          Console.WriteLine(file.FullName);
          File.WriteAllText(file.FullName, String.Join("\n", lines.ToArray()));
        }
      }
    });
  }
}