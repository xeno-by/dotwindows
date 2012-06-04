// build this with "csc /t:winexe language.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"http://groups.google.com/group/scala-language");
  }
}