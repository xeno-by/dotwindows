// build this with "csc /t:winexe api.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    Process.Start(@"http://www.sublimetext.com/docs/2/api_reference.html");
  }
}