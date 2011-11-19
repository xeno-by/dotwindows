// build this with "csc /t:exe /debug+ myke*.cs"

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

public class App {
  public static int Main(String[] args) {
    var status = Config.parse(args);
    if (status != 0) return status;

    if (Config.verbose) {
      Config.print();
    }

    var originalTarget = Config.target;
    var t_all = Connectors.all;
    if (Config.verbose) {
      Console.println();
      Console.println("connectors: ");
      Help.printConnectors();
    }

    while (Config.target != null) {
      if (Config.verbose) {
        Console.println();
        Console.println("considering target {0}", Config.target);
      }

      foreach (var t_conn in t_all) {
        if (Config.verbose) {
          Console.print("* {0}... ", t_conn.name());
        }

        var conn = t_conn.instantiate();
        var accepts = conn != null && conn.accept();
        if (Config.verbose) {
          if (accepts) Console.println("[A]");
          else {
            if (!Connectors.lastBindArgsOk) {
              // [R] is logged directly at bindArgs
            } else {
              Console.println("[R]");
              Console.println("reason: {0}.accept() has returned false", t_conn.name());
            };
          }
        }

        if (accepts) {
          var action = Config.action;
          if (Config.verbose) {
            Console.println();
            Console.println("========================================");
            Console.println("{0} will now perform {1} upon {2}", conn.name(), Config.action, Config.target);
            Console.println("========================================");
            Console.println();
          }

          Console.setupHistory(String.Format("{0}_{1}", conn.name(), action));
          return (int)conn.action(action);
        }
      }

      Config.target = Path.GetDirectoryName(Config.target);
    }

    if (Config.verbose) {
      Console.println();
    }

    Console.println("error: no connector accepted the target {0}", originalTarget);
    return -1;
  }
}

public static class Console {
  public static void print(String format, params Object[] objs) {
    System.Console.Write(String.Format(format, objs));
  }

  public static void print(String obj) {
    System.Console.Write(obj);
  }

  public static void print(Object obj) {
    System.Console.Write(obj);
  }

  public static void println(String format, params Object[] objs) {
    System.Console.WriteLine(String.Format(format, objs));
  }

  public static void println(String obj) {
    System.Console.WriteLine(obj);
  }

  public static void println(Object obj) {
    System.Console.WriteLine(obj);
  }

  public static void println() {
    System.Console.WriteLine();
  }

  public static String readln(String prompt = null) {
    if (prompt != null && prompt != String.Empty) print(prompt + ": ");
    return System.Console.ReadLine();
  }

  public static void setupHistory(String key) {
    // todo
  }

  private static ProcessStartInfo parse(String command) {
    command = command.TrimStart(new []{' '});

    var i = 0;
    var buf = new StringBuilder();
    var in_quote = false;
    Func<bool> prev_slash = () => i == 1 ? false : command[i - 2] == '\\';
    while (i < command.Length) {
      var c = command[i++];

      if (c == '\"') {
        if (prev_slash()) {
          buf.Remove(buf.Length - 1, 1);
          buf.Append(c);
        } else {
          if (in_quote) {
            in_quote = false;
            break;
          } else {
            in_quote = true;
          }
        }
      } else if (c == '\\') {
        if (prev_slash()) {
          buf.Remove(buf.Length - 1, 1);
          buf.Append(c);
        } else {
          buf.Append(c);
        }
      } else if (c == ' ') {
        if (prev_slash()) {
          buf.Remove(buf.Length - 1, 1);
          buf.Append(c);
        } else {
          if (in_quote) {
            buf.Append(c);
          } else {
            break;
          }
        }
      } else {
        buf.Append(c);
      }
    }

    var fileName = buf.ToString();
    var arguments = command.Substring(i);
    String workingDirectory = ".";
    if (Directory.Exists(Config.target)) workingDirectory = Config.target;
    if (File.Exists(Config.target)) workingDirectory = Path.GetDirectoryName(Config.target);
    return new ProcessStartInfo{FileName = fileName, Arguments = arguments, WorkingDirectory = workingDirectory};
  }

  private static int execute(String command, bool interactive) {
    var psi = parse(command);
    if (Config.verbose) {
      Console.println("psi: filename = {0}, arguments = {1}, home = {2}", psi.FileName, psi.Arguments, psi.WorkingDirectory);
      Console.println();
      Console.println("========================================");
      Console.println("The command will now be executed by cmd.exe");
      Console.println("========================================");
      Console.println();
    }

    psi.Arguments = "/C \"" + psi.FileName + "\" " + psi.Arguments;
    psi.FileName = "cmd.exe";
    psi.UseShellExecute = false;

    var p = new Process();
    p.StartInfo = psi;

    if (p.Start()) {
      p.WaitForExit();
      return p.ExitCode;
    } else {
      return -1;
    }
  }

  public static int batch(String command) {
    return execute(command, false);
  }

  public static int batch(String command, Arguments arguments) {
    return batch(command + " " + String.Join(" ", arguments.ToArray()));
  }

  public static int interactive(String command) {
    return execute(command, true);
  }

  public static int interactive(String command, Arguments arguments) {
    return interactive(command + " " + String.Join(" ", arguments.ToArray()));
  }

  public static int ui(String command) {
    return -1;
  }

  public static int ui(String command, Arguments arguments) {
    return ui(command + " " + String.Join(" ", arguments.ToArray()));
  }
}

public static class Config {
  public static bool dryrun;
  public static bool verbose;
  public static String action;
  public static String target;
  public static Arguments args;

  public static int parse(String[] args) {
    var flags = args.TakeWhile(arg => arg.StartsWith("/")).Select(flag => flag.ToUpper()).ToList();
    args = args.SkipWhile(arg => arg.StartsWith("/")).ToArray();
    Config.dryrun = flags.Contains("/X") || !flags.Contains("/D");
    Config.verbose = flags.Contains("/V");

    var action = args.Take(1).ElementAtOrDefault(0) ?? "compile";
    if (File.Exists(action) || Directory.Exists(action)) {
      if (Config.verbose) {
        Console.println("[WARNING! I'M TRYING TO BE SMARTASS!!] Action {0} read from the first command-line arg is very similar to a filename.", action);
        Console.println("Thus, I will think that {0} represents the target, and will infer that action defaults to a connector-specific value", action);
      }

      action = "default";
    } else {
      args = args.Skip(1).ToArray();
    }

    if (action == null || action == "/?" || action == "-help" || action == "--help") {
      Help.printUsage();
      return -1;
    }

    Config.action = action;

    var target = args.Take(1).ElementAtOrDefault(0) ?? ".";
    args = args.Skip(1).ToArray();
    if (target == null || target == "/?" || target == "-help" || target == "--help") {
      Help.printUsage(action);
      return -1;
    }

    // don't check for file existence - connector might actually create that non-existent file/directory
    Config.target = Path.GetFullPath(target);
    Config.args = new Arguments(args.ToList());

    return 0;
  }

  public static void print() {
    Console.println("config: dryrun = {0}, verbose = {1}, action = {2}, target = {3}, args = {4}", dryrun, verbose, action, target, String.Join(", ", args));
  }
}

public static class Help {
  public static void printUsage(String action = null) {
    if (action == null) {
      Console.println();
      Console.println("myke [/X] [/D] [/V] [action] [target] [args...]");
      Console.println("  action :  one of \"compile\", \"rebuild\", \"run\", \"repl\", \"test\"; defaults to \"compile\"");
      Console.println("  target :  a single file or directory name that will hint what to do next; defaults to \".\"");
      Console.println("             you cannot provide multiple input file/directory names (e.g. as in scalac)");
      Console.println("             this is by design to be bijectively compatible with single-file editors");
      Console.println("             if you do need this, consider configuring your project with some build system");
      Console.println("             and hand-editing myke.cs to integrate with it (e.g. this is how it was done for sbt)");
      Console.println("  args   :  custom data that will be passed to the handler of the command; defaults to \"\"");
      Console.println();
      Console.println("flags:");
      Console.println("  /X     :  execute the command provided by the command-line; enabled by default");
      Console.println("  /D     :  runs all prerequisites (e.g. compilation) and prints the command-line of the continuation");
      Console.println("             this is useful if the continuation is interactive, and myke is incapable of emulating it");
      Console.println("  /V     :  enables verbose mode of execution; disabled by default");
      Console.println();
      Console.println("see https://raw.github.com/xeno-by/dotwindows/master/Scripts/Scripts/myke.cs for more information");
      Console.println("if you scroll past console and configuration logic, you will find connectors to various build engines");
      Console.println();
      Console.println("connectors:");
    } else {
      Console.println();
      Console.println("action \"{0}\" is supported by the following connectors:", Config.action);
      Console.println();
    }

    printConnectors(action);
  }

  public static void printConnectors(String action = null) {
    var connectors = Connectors.all.Where(connector => action == null || connector.actions().ContainsKey(action)).ToList();
    var maxlen = connectors.Max(connector => connector.name().Length);

    for (var i = 0; i < connectors.Count; ++i) {
      var connector = connectors[i];
      Console.println("* {0} (supports {1})", connector.name(), String.Join(", ",
        connector.actions().Select(kvp => kvp.Key != "default" ? kvp.Key : String.Format("{0} = {1}", kvp.Key, kvp.Value.name()))));
      Console.println(connector.description());
    }
  }
}

public class ConnectorAttribute : Attribute {
  public String name;
  public String description;
  public double priority;
}

public class ActionAttribute : Attribute {
  public String name;

  public ActionAttribute() {
  }

  public ActionAttribute(String name) {
    this.name = name;
  }
}

public class DefaultAttribute : Attribute {
}

public static class Connectors {
  public static List<Type> all { get {
    var root = Assembly.GetExecutingAssembly();
    var t_connectors = root.GetTypes().Where(t => t.IsDefined(typeof(ConnectorAttribute), true)).ToList();
    return t_connectors.OrderBy(connector => connector.priority()).ToList();
  } }

  public static String name(this Object connector) {
    return connector.GetType().name();
  }

  public static String name(this Type connector) {
    var attr = connector.GetCustomAttributes(typeof(ConnectorAttribute), true).Cast<ConnectorAttribute>().Single();
    return attr.name;
  }

  public static String name(this MethodInfo meth) {
    var attr = meth.GetCustomAttributes(typeof(ActionAttribute), true).Cast<ActionAttribute>().Single();
    return attr.name ?? meth.Name;
  }

  public static String description(this Object connector) {
    return connector.GetType().description();
  }

  public static String description(this Type connector) {
    var attr = connector.GetCustomAttributes(typeof(ConnectorAttribute), true).Cast<ConnectorAttribute>().Single();
    return attr.description;
  }

  public static double priority(this Object connector) {
    return connector.GetType().priority();
  }

  public static double priority(this Type connector) {
    var attr = connector.GetCustomAttributes(typeof(ConnectorAttribute), true).Cast<ConnectorAttribute>().Single();
    return attr.priority;
  }

  public static Dictionary<String, MethodInfo> actions(this Type connector) {
    var methods = connector.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.IsDefined(typeof(ActionAttribute), true)).ToList();

    var map = methods.ToDictionary(meth => {
      var attr = meth.GetCustomAttributes(typeof(ActionAttribute), true).Cast<ActionAttribute>().Single();
      return attr.name ?? meth.Name;
    }, meth => meth);

    methods.ForEach(meth => {
      var attr = meth.GetCustomAttributes(typeof(DefaultAttribute), true).Cast<DefaultAttribute>().SingleOrDefault();
      if (attr != null) map.Add("default", meth);
    });

    return map;
  }

  public static Object instantiate(this Type connector) {
    var ctor = connector.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Single();
    var args = ctor.bindArgs();
    return args == null ? null : ctor.Invoke(args);
  }

  public static bool accept(this Object connector) {
    var method = connector.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == "accept").Single();
    var args = method.bindArgs();
    return args == null ? false : (bool)method.Invoke(connector, args);
  }

  public static Object action(this Object connector, String action) {
    var method = connector.GetType().actions()[action];
    var args = method.bindArgs();
    return method.Invoke(connector, args);
  }

  public static bool lastBindArgsOk = false;
  private static Object[] bindArgs(this MethodBase method) {
    try {
      lastBindArgsOk = true;
      return method.GetParameters().Select<ParameterInfo, Object>(p => {
        if (p.ParameterType == typeof(FileInfo)) {
          var file = new FileInfo(Config.target);
          if (!file.Exists) throw new FileNotFoundException(Config.target);
          return file;
        } else if (p.ParameterType == typeof(DirectoryInfo)) {
          var dir = new FileInfo(Config.target);
          if (!dir.Exists) throw new DirectoryNotFoundException(Config.target);
          return dir;
        } else if (p.ParameterType == typeof(Lines)) {
          var file = new FileInfo(Config.target);
          if (!file.Exists) throw new FileNotFoundException(Config.target);
          return new Lines(file, File.ReadAllLines(file.FullName).ToList());
        } else if (p.ParameterType == typeof(Arguments)) {
          return Config.args;
        } else {
          throw new NotSupportedException(String.Format("cannot bind parameter {0} of type {1}", p.Name, p.ParameterType.FullName));
        }
      }).ToArray();
    } catch (Exception ex) {
      if (Config.verbose) {
        Console.println("[R]");
        Console.println("reason: {0} ({1})", ex.GetType().Name, ex.Message);
      }

      lastBindArgsOk = false;
      return null;
    }
  }
}

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy("System.Collections.Generic.Mscorlib_CollectionDebugView`1,mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089")]
[DebuggerNonUserCode]
public abstract class BaseList<T> : IList<T> {
  protected abstract IEnumerable<T> Read();

  public virtual bool IsReadOnly { get { return true; } }
  protected virtual void InsertAt(int index, T el) {}
  protected virtual void UpdateAt(int index, T el) {}
  public virtual void RemoveAt(int index) {}

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
  public IEnumerator<T> GetEnumerator() {
    return Read().GetEnumerator();
  }

  public void Add(T item) {
    if (IsReadOnly) throw new NotSupportedException("cannot add to a readonly list");
    Insert(Count, item);
  }

  public void Clear() {
    if (IsReadOnly) throw new NotSupportedException("cannot clear a readonly list");
    for (var i = 0; i < Count; ++i) RemoveAt(0);
  }

  public bool Contains(T item) {
    return IndexOf(item) != -1;
  }

  void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
    Read().ToArray().CopyTo(array, arrayIndex);
  }

  public bool Remove(T item) {
    if (IsReadOnly) throw new NotSupportedException("cannot remove from a readonly list");

    var index = IndexOf(item);
    if (index == -1) {
      return false;
    } else {
      RemoveAt(index);
      return true;
    }
  }

  public int Count {
    get { return Read().Count(); }
  }

  public int IndexOf(T item) {
    return Read().ToList().IndexOf(item);
  }

  public void Insert(int index, T item) {
    if (IsReadOnly) throw new NotSupportedException("cannot insert into a readonly list");
    InsertAt(index, item);
  }

  public T this[int index] {
    get { return Read().ElementAt(index); }
    set {
      if (IsReadOnly) throw new NotSupportedException("cannot update a readonly list");
      UpdateAt(index, value);
    }
  }
}

public class Lines : BaseList<String> {
  public FileInfo file;
  private List<String> lines;

  public Lines(FileInfo file, List<String> lines) {
    this.file = file;
    this.lines = lines;
  }

  protected override IEnumerable<String> Read() {
    return lines;
  }
}

public class Arguments : BaseList<String> {
  private List<String> arguments;

  public Arguments(List<String> arguments) {
    this.arguments = arguments;
  }

  protected override IEnumerable<String> Read() {
    return arguments;
  }
}
