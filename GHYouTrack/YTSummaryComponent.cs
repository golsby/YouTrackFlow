using System;
using System.Text;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using YouTrack;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;

namespace GHYouTrack
{
  public class YTSummaryComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the YTSummaryComponent class.
    /// </summary>
    public YTSummaryComponent()
      : base("YTSummary", "YouTrack Summary",
          "Summarize YouTrack Issues",
          "YouTrack", "Summary")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Issues", "i", "YouTrack Issues", GH_ParamAccess.tree);
      pManager.AddTextParameter("Attribute", "a", "Attribute to Summarize", GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddNumberParameter("Count", "c", "Total Issue Count", GH_ParamAccess.item);
      pManager.AddTextParameter("Output", "out", "output", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      GH_Structure<IGH_Goo> issue_tree = new GH_Structure<IGH_Goo>();
      string attributeName = "";
      DA.GetDataTree("Issues", out issue_tree);
      DA.GetData("Attribute", ref attributeName);

      StringBuilder output = new StringBuilder();
      int total = 0;

      for (int b=0; b<issue_tree.Branches.Count; b++)
      {
        var summary = new StringCounter();
        foreach (var gh_issue in issue_tree[b])
        {
          Issue issue = ((GH_ObjectWrapper)gh_issue).Value as Issue;
          if (issue == null)
            continue;

          total += 1;
          var value = issue.GetPropertyValue(attributeName);
          summary.Increment(value);
        }
        output.Append(string.Format("[Path {0}]\r\n", b));
        var keys = new List<string>();
        keys.AddRange(summary.Keys);
        keys.Sort();
        foreach (var key in keys)
        {
          output.Append(key).Append(": ").Append(summary[key].ToString()).Append("\n");
        }
        output.Append("\r\n\r\n");
      }

      DA.SetData(0, total);
      DA.SetData(1, output.ToString());
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        return Resources.Summary;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("{2e5d2f4b-1242-4ef9-9d7c-24e3021a4caa}"); }
    }
  }
}