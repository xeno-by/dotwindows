// build this with "csc /t:winexe pins.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"D:\Dropbox\Scratchpad\Stockage\Programming in Scala 2nd.pdf");
  }
}