// build this with "csc /t:winexe iss.cs"

using System;
using System.Diagnostics;
using System.Linq;

public class App {
  [STAThread]
  public static void Main(String[] args) {
    var num = args.ElementAtOrDefault(0);
    if (num == null) return;

    num = num.ToUpper();
    if (num.StartsWith("SI")) num = num.Substring(2);
    if (num.StartsWith("-")) num = num.Substring(1);
    int value;
    if (!int.TryParse(num, out value)) return;
    num = num.PadLeft(4, '0');
    if (num.Length != 4) return;

    var url = "https://issues.scala-lang.org/browse/SI-" + num;
    Process.Start(url);
  }
}