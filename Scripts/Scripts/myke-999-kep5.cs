// build this with "csc /r:ZetaLongPaths.dll /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

[Connector(name = "kep5", priority = 999, description =
  "Wraps the development workflow of the stable mirror or project Kepler.\r\n" +
  "Uses ant for building, itself for a repl, runs Reflection and doesn't support tests yet.")]

public class Kep5 : Kep {
  public override String project { get { return @"%PROJECTS%\Kepler5".Expand(); } }
  public override String profile { get { return "fastlocker"; } }
  //public override String profile { get { return "build"; } }

  public Kep5() : base() {}
  public Kep5(FileInfo file) : base(file) {}
  public Kep5(DirectoryInfo dir) : base(dir) {}
}