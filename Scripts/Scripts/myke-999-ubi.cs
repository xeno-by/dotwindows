// build this with "csc /t:exe /out:myke.exe /debug+ myke*.cs"

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

[Connector(name = "ubi", priority = 999.1, description = "Compiles ubi scripts")]

public class Ubi : Csc {
  public Ubi(FileInfo file, Lines lines) : base(file, lines) {
  }
}