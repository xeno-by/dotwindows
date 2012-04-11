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

[Connector(name = "kep5", priority = 999, description =
  "Wraps the development workflow of the stable mirror or project Kepler.")]

public class Kep5 : Kep {
  public override String project { get { return @"%PROJECTS%\Kepler5".Expand(); } }
  //public override String profile { get { return "fastlocker"; } }
  public virtual String profile { get { return "locker.unlock locker.done"; } }
  //public override String profile { get { return "build"; } }

  public Kep5() : base() {}
  public Kep5(FileInfo file, Arguments arguments) : base(file, arguments) {}
  public Kep5(DirectoryInfo dir, Arguments arguments) : base(dir, arguments) {}
}