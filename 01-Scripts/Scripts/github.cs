// build this with "csc /t:winexe github.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"http://rubydoc.info/github/peter-murach/github/master/frames");
  }
}