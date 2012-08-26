// build this with "csc /t:exe clojure.cs"

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

public class App {
  public static void Main(String[] args) {
    Process.Start("java.exe", "-cp D:/Dropbox/Software/Windows/Development/Clojure/clojure-1.4.0.jar clojure.main");
  }
}