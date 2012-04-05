using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class App {
  public static void Main(String[] args) {
    if (args.Length != 2) {
      Console.WriteLine(@"usage: ensimea ADAPTEE_PORT ADAPTED_PORT");
      return;
    }

    var adapteePort = int.Parse(args[0]);
    var adaptedPort = int.Parse(args[1]);

    var serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), adaptedPort);
    serverSocket.Start();
    Console.WriteLine("Adapter listening on {0}...", adaptedPort);

    while (true) {
      TcpClient adapterSocket = serverSocket.AcceptTcpClient();
      Socket adapteeSocket = null;

      new Thread(() => {
        try {
          var adapteeEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), adapteePort);
          adapteeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          adapteeSocket.Connect(adapteeEndpoint);

          if (adapteeSocket.Connected) {
            var adapter = new SwankAdapter(adapterSocket.GetStream());
            var adaptee = new SwankAdaptee(new NetworkStream(adapteeSocket));
            adapter.Connect(adaptee);
            adaptee.Connect(adapter);
            adapter.KickOff();
            adaptee.KickOff();
            while (adapter.active && adaptee.active) {
              Thread.Sleep(1000);
            }
          }
        } finally {
          adapterSocket.Close();
          adapteeSocket.Close();
        }

      }).Start();
    }
  }

  public class SwankAdaptee {
    public Stream stream;
    public StreamReader reader;
    public StreamWriter writer;
    public SwankAdaptee(Stream stream) {
      this.stream = stream;
      this.reader = new StreamReader(stream);
      this.writer = new StreamWriter(stream);
    }

    public SwankAdapter adapter;
    public bool active { get { return stream.CanRead; } }
    public void Connect(SwankAdapter adapter) { this.adapter = adapter; }

    public void Write(String s) {
      writer.Write(s.Length.ToString("x6"));
      writer.Write(s);
      writer.Flush();
    }

    public String Read() {
      var slen = "" + (char)reader.Read() + (char)reader.Read() + (char)reader.Read() + (char)reader.Read() + (char)reader.Read() + (char)reader.Read();
      int ilen;
      if (!int.TryParse(slen, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ilen)) throw new Exception(String.Format("broken swank protocol: not a hex header {0}", slen));
      var buf = new char[ilen];
      var actual = reader.Read(buf, 0, ilen);
      if (actual != ilen) throw new Exception(String.Format("broken swank protocol: expected {0} chars, actual {1} chars", ilen, actual));
      return new String(buf);
    }

    public void KickOff() {
      new Thread(() => {
        try {
          while (stream.CanRead) {
            adapter.Write(Read());
          }
        } catch (Exception ex) {
          Console.WriteLine("[swank adaptee] " + ex);
        }
      }).Start();
    }
  }

  public class SwankAdapter {
    public Stream stream;
    public StreamReader reader;
    public StreamWriter writer;
    public SwankAdapter(Stream stream) {
      this.stream = stream;
      this.reader = new StreamReader(stream);
      this.writer = new StreamWriter(stream);
    }

    public void Write(String s) {
      writer.WriteLine(s);
      writer.Flush();
    }

    public String Read() {
      return reader.ReadLine();
    }

    public SwankAdaptee adaptee;
    public bool active { get { return stream.CanRead; } }
    public void Connect(SwankAdaptee adaptee) { this.adaptee = adaptee; }

    public void KickOff() {
      new Thread(() => {
        try {
          var helo = "(:swank-rpc (swank:connection-info) 1)";
          this.Write(helo);
          adaptee.Write(helo);

          //var init = "(:swank-rpc (swank:init-project (:sources (\"d:/Dropbox/Scratchpad/Scala\") :target \"d:/Dropbox/Scratchpad/Scala\")) 2)";
          //var init = "(:swank-rpc (swank:init-project (:sources (\"C:/Projects/Kepler/src/library\" \"C:/Projects/Kepler/src/compiler\") :compile-jars (\"C:/Projects/Kepler/lib\") :target \"C:/Projects/Kepler/build\")) 2)";
          var init = "(:swank-rpc (swank:init-project (:sources (\"C:/Projects/Kepler/src/library\" \"C:/Projects/Kepler/src/compiler\") :target \"C:/Projects/Kepler/build\")) 2)";
          //var init = "(:swank-rpc (swank:init-project " + File.ReadAllText(@"D:\Dropbox\Scratchpad\Scala\.ensime") + ") 2)";
          this.Write(init);
          adaptee.Write(init);

          //var compile = "(:swank-rpc (swank:builder-init) 3)";
          //this.Write(compile);
          //adaptee.Write(compile);

          while (stream.CanRead) {
            adaptee.Write(Read());
          }
        } catch (Exception ex) {
          Console.WriteLine("[swank adapter] " + ex);
        }
      }).Start();
    }
  }
}