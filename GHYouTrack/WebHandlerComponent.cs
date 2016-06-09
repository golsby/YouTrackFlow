using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHYouTrack
{
  public class GH_WebHandlerData
  {
    public string Path;
    public string Content;
  }

  public class WebHandlerComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the WebHandlerComponent class.
    /// </summary>
    public WebHandlerComponent()
      : base("WebHandlerComponent", "Handler",
          "Handles an HTTP request for one path",
          "YouTrack", "HTTP")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddTextParameter("Path", "p", "Path to handle", GH_ParamAccess.item, "/");
      pManager.AddTextParameter("Content", "c", "Content to serve", GH_ParamAccess.item, "<b>Hello World</b>");
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Handler", "h", "HTTP Handler", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      string path = "";
      string content = "";
      DA.GetData("Path", ref path);
      DA.GetData("Content", ref content);
      var handler = new GH_WebHandlerData();
      handler.Content = content;
      handler.Path = path;
      DA.SetData("Handler", handler);
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        return Resources.ServerHandler;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("{6c0602fd-5640-4d08-99d1-e92060f59814}"); }
    }
  }
}