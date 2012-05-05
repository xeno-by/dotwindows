// build this with "csc /r:Microsoft.VisualBasic.dll /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "kur", priority = 999, description =
  "Wraps the development workflow of the Kepler under refactoring.")]

public class Kur : Kep {
  public override String project { get { return @"%PROJECTS%\KeplerUnderRefactoring".Expand(); } }

  public Kur() : base() {}
  public Kur(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public Kur(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}
}