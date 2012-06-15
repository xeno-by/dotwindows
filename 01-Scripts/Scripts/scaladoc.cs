// build this with "csc /t:winexe scaladoc.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"http://www.scala-lang.org/archives/downloads/distrib/files/nightly/docs/library/index.html#package");
  }
}