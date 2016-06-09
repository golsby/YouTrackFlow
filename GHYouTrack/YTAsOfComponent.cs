using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using YouTrack;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace GHYouTrack
{
  public class YTAsOfComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the YTAsOfComponent class.
    /// </summary>
    public YTAsOfComponent()
      : base("YT As Of", "As Of",
          "YouTrack issues as of a date",
          "YouTrack", "Query")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Issues", "i", "YouTrack Issues", GH_ParamAccess.list);
      pManager.AddTimeParameter("Timestamp", "t", "Issues as of this date", GH_ParamAccess.list);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddNumberParameter("Count", "c", "Count of issues", GH_ParamAccess.list);
      pManager.AddGenericParameter("Issues", "i", "Filtered issues", GH_ParamAccess.tree);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      IssueCollection issues = new IssueCollection();
      List<DateTime> timestamps = new List<DateTime>(); 
      DA.GetDataList<Issue>(0, issues);
      DateTime timestamp = DateTime.Now;

      DataTree<Issue> results = new DataTree<Issue>();
      List<int> counts = new List<int>();

      DA.GetDataList(1, timestamps);
      for (int i=0; i<timestamps.Count; i++)
      {
        timestamp = timestamps[i];

        IssueCollection issues_as_of = issues.AsOf(timestamp);
        GH_Path path = new GH_Path(i);
        results.AddRange(issues_as_of, path);
        counts.Add(issues_as_of.Count);
      }

      DA.SetDataList(0, counts);
      DA.SetDataTree(1, results);
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        return Resources.Calendar;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("{f971bfa6-5c1d-459a-bf60-1c04a01e0a31}"); }
    }
  }
}