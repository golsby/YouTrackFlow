using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using YouTrack;

namespace GHYouTrack
{
  public class YTQueryComponent : GH_Component
  {
    IssueCollection _issues = null;
    string _filter = "";

    /// <summary>
    /// Initializes a new instance of the YTQueryComponent class.
    /// </summary>
    public YTQueryComponent()
      : base("YTQuery", "YouTrack Query",
          "Query YouTrack web site for issues",
          "YouTrack", "Query")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddBooleanParameter("Run", "run", "Run this query", GH_ParamAccess.item, false);
      pManager.AddTextParameter("Project", "P", "YouTrack Project", GH_ParamAccess.item, "RH");
      pManager.AddTextParameter("ReleaseTarget", "RT", "Release Target", GH_ParamAccess.item, "6.0");
      pManager.AddTextParameter("Search", "S", "Search", GH_ParamAccess.item, "");
      pManager.AddTextParameter("UpdatedBefore", "UB", "Updated Before", GH_ParamAccess.item, "");
      pManager.AddTextParameter("UpdatedAfter", "UA", "Updated After", GH_ParamAccess.item, "");
      pManager.AddTextParameter("CreatedBefore", "CB", "Created Before", GH_ParamAccess.item, "");
      pManager.AddTextParameter("CreatedAfter", "CA", "Created After", GH_ParamAccess.item, "");
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("Query", "q", "Query String", GH_ParamAccess.item);
      pManager.AddGenericParameter("Issues", "i", "Filtered issues", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      bool run = false;
      DA.GetData("Run", ref run);
      if (!run)
        return;

      string project = "", releaseTarget = "", search = "";
      string sUpdatedBefore = "", sUpdatedAfter = "", sCreatedBefore = "", sCreatedAfter = "";
      DateTime updatedBefore, updatedAfter, createdBefore, createdAfter;
      updatedBefore = updatedAfter = createdBefore = createdAfter = DateTime.MinValue;
      DA.GetData("Project", ref project);
      DA.GetData("ReleaseTarget", ref releaseTarget);
      DA.GetData("Search", ref search);

      DA.GetData("UpdatedBefore", ref sUpdatedBefore);
      DA.GetData("UpdatedAfter", ref sUpdatedAfter);
      DA.GetData("CreatedBefore", ref sCreatedBefore);
      DA.GetData("CreatedAfter", ref sCreatedAfter);

      if (!string.IsNullOrWhiteSpace(sUpdatedBefore))
        updatedBefore = DateTime.Parse(sUpdatedBefore);
      if (!string.IsNullOrWhiteSpace(sUpdatedAfter))
        updatedAfter = DateTime.Parse(sUpdatedAfter);
      if (!string.IsNullOrWhiteSpace(sCreatedBefore))
        createdBefore = DateTime.Parse(sCreatedBefore);
      if (!string.IsNullOrWhiteSpace(sCreatedAfter))
        createdAfter = DateTime.Parse(sCreatedAfter);

      var q = new Query();
      q.Project = project;
      q.ReleaseTarget = releaseTarget;
      q.Filter = search;
      q.UpdatedBefore = updatedBefore;
      q.UpdatedAfter = updatedAfter;
      q.CreatedBefore = createdBefore;
      q.CreatedAfter = createdAfter;

      if (q.ToString() != _filter || _issues == null)
      {
        _issues = q.FetchAllIssues();
        _filter = q.ToString();
      }

      DA.SetData(0, q.ToString());
      DA.SetDataList(1, _issues);
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        return Resources.Query;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("{d09d2854-9c0f-4986-aefc-0b64cc7738b7}"); }
    }
  }
}