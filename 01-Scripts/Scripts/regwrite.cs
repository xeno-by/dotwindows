// build this with "csc /t:exe regwrite.cs"

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

public class App {
  public static int Main(String[] args) {
    var map = new Dictionary<String, RegistryValueKind>();
    map.Add("REG_SZ", RegistryValueKind.String);
    map.Add("REG_EXPAND_SZ", RegistryValueKind.ExpandString);
    map.Add("REG_BINARY", RegistryValueKind.Binary);
    map.Add("REG_DWORD", RegistryValueKind.DWord);
    map.Add("REG_MULTI_SZ", RegistryValueKind.MultiString);
    map.Add("REG_QWORD", RegistryValueKind.QWord);

    if ((args.Length == 1 && (args[0] == "/?" || args[0] == "--help")) ||
        (args.Length != 2 && args.Length != 3) ||
        (args.Length == 3 && !map.ContainsKey(args[2].ToUpper()))) {
      Console.WriteLine(@"usage: regwrite PATH\TO\VALUE VALUE [TYPE]");
      Console.WriteLine("possible values for TYPE are " + String.Join(", ", map.Keys));
      Console.WriteLine("source: http://en.wikipedia.org/wiki/Windows_Registry#Keys_and_values");
      return -1;
    }

    var full = args[0].Replace("/", "\\");
    var iof = full.LastIndexOf("\\");
    if (iof == -1) return -1;

    var key = full.Substring(0, iof);
    var value = full.Substring(iof + 1);

    iof = key.IndexOf("\\");
    if (iof == -1) return -1;
    var hive = key.Substring(0, iof);
    key = key.Substring(iof + 1);

    RegistryKey rhive = null;
    switch (hive) {
      case "HKCU":
      case "HKEY_CURRENT_USER":
        rhive = Registry.CurrentUser;
        break;

      case "HKLM":
      case "HKEY_LOCAL_MACHINE":
        rhive = Registry.LocalMachine;
        break;

      case "HKCR":
      case "HKEY_CLASSES_ROOT":
        rhive = Registry.ClassesRoot;
        break;

      case "HKU":
      case "HKEY_USERS":
        rhive = Registry.Users;
        break;

      case "HKPD":
      case "HKEY_PERFORMANCE_DATA":
        rhive = Registry.PerformanceData;
        break;

      case "HKCC":
      case "HKEY_CURRENT_CONFIG":
        rhive = Registry.CurrentConfig;
        break;

      default:
          return -1;
    }

    var rkey = rhive.OpenSubKey(key, true) ?? rhive.CreateSubKey(key);
    var rvalue = rkey.GetValueNames().FirstOrDefault(name => name == value);
    var type = args.Length == 3 ? map[args[2].ToUpper()] : (rvalue != null ? rkey.GetValueKind(rvalue) : RegistryValueKind.String);
    rkey.SetValue(value, args[1], type);

    return 0;
  }
}