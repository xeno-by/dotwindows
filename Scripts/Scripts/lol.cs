// build this with "csc /t:winexe lol.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"http://letoverlambda.com/index.cl/toc");
  }
}