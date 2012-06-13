// build this with "csc /t:winexe gsens.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"https://github.com/sublimescala/sublime-ensime");
  }
}