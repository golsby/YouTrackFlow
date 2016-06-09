using System;
using System.Net;
using System.Threading;
using System.Text;

using Grasshopper.Kernel;
using System.Collections.Generic;

namespace GHYouTrack
{
  public class AsyncServer
  {
    HttpListener m_listener = new HttpListener();
    bool m_stopping = false;
    public AsyncServer()
    {
      var listener = new HttpListener();
      _port = 41414;

    }

    public void Start()
    {
      new Thread(() =>
      {
        m_listener.Prefixes.Add(Url);
        m_listener.Start();
        while (true)
        {
          try {
            if (m_stopping)
              break;
            var context = m_listener.GetContext();
            ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
          }
          catch (HttpListenerException)
          {
            break;
          }
        }
      }).Start();
    }

    public void Stop()
    {
      m_stopping = true;
      m_listener.Close();
    }

    int _port;

    public int Port
    {
      get { return _port; }
      set { _port = value; }
    }
    public string Url
    {
      get
      {
        return string.Format("http://localhost:{0}/", _port);
      }
    }
    public List<GH_WebHandlerData> Handlers { get; set; }

    private void HandleRequest(object state)
    {
      try
      {
        var context = (HttpListenerContext)state;
        var req = context.Request;
        string content = string.Format("404 - no handler for {0}", req.RawUrl);
        
        foreach (var handler in Handlers)
        {
          if (0 == string.Compare(handler.Path, req.RawUrl, true))
          {
            content = handler.Content;
            break;
          }
        }

        context.Response.StatusCode = 200;
        context.Response.SendChunked = true;
        context.Response.Headers[HttpResponseHeader.CacheControl] = "no-cache, no-store, must-revalidate";
        context.Response.Headers[HttpResponseHeader.Pragma] = "no-cache";
        context.Response.Headers[HttpResponseHeader.Expires] = "0";

        var bytes = Encoding.UTF8.GetBytes(content);
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Close();
      }
      catch (Exception)
      {
        // Client disconnected or some other error - ignored for this example
      }
    }
  }

  public class WebServerComponent : GH_Component
  {
    static Dictionary<int, AsyncServer> m_servers = new Dictionary<int, AsyncServer>();

    /// <summary>
    /// Initializes a new instance of the WebServerComponent class.
    /// </summary>
    public WebServerComponent()
      : base("WebServerComponent", "WebServer",
          "Host a single HTML file in a web server",
          "YouTrack", "HTTP")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddIntegerParameter("Port", "p", "HTTP Port", GH_ParamAccess.item, 41414);
      pManager.AddGenericParameter("Handlers", "h", "outputs from GH_WebHandlerComponent", GH_ParamAccess.list);

    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("URL", "url", "URL to access this server", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      int port = 41414;
      DA.GetData("Port", ref port);
      List<GH_WebHandlerData> handlers = new List<GH_WebHandlerData>();
      DA.GetDataList("Handlers", handlers);

      if (m_servers.ContainsKey(port))
        m_servers[port].Stop();

      var _server = new AsyncServer();
      _server.Port = port;
      _server.Handlers = handlers;

      DA.SetData("URL", _server.Url);
      m_servers[port] = _server;

      _server.Start();
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        return Resources.Server;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("{678f282a-7adf-4699-b6be-da4204c4d9f2}"); }
    }
  }
}