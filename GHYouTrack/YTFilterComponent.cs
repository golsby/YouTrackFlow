using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using YouTrack;
using System.Reflection;

namespace GHYouTrack
{
  public class YTFilterComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the YTFilter class.
    /// </summary>
    public YTFilterComponent()
      : base("YTFilter", "Filter Component",
          "Filter YouTrack Issues",
          "YouTrack", "Query")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Issues", "i", "YouTrack Issues", GH_ParamAccess.list);
      pManager.AddTextParameter("Field", "f", "Field to filter by", GH_ParamAccess.item);
      pManager.AddTextParameter("Value", "v", "Value to include in results", GH_ParamAccess.list);
      pManager.AddBooleanParameter("SearchHistory", "h", "Include historical states in filter", GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddNumberParameter("Count", "c", "Count of issues", GH_ParamAccess.item);
      pManager.AddGenericParameter("Issues", "i", "Filtered issues", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      IssueCollection issues = new IssueCollection();
      string field = "";
      List<string> values = new List<string>();
      bool searchHistory = false;
      DA.GetDataList<Issue>(0, issues);
      DA.GetData(1, ref field);
      DA.GetDataList(2, values);
      DA.GetData(3, ref searchHistory);

      IssueCollection filtered = new IssueCollection();

      foreach(var issue in issues)
      {
        bool issueAdded = false;
        if (issue == null)
          continue;
        string prop = issue.GetPropertyValue(field);
        foreach (string value in values)
        {
          if (0 == string.Compare(prop, value, true))
          {
            filtered.Add(issue);
            issueAdded = true;
          }
        }
        if (searchHistory)
        {
          foreach (var change in issue.changes)
          {
            if (0 == string.Compare(change.Action, field))
            {
              foreach (string value in values)
              {
                if (0 == string.Compare(change.To, value, true))
                {
                  if (!issueAdded)
                  {
                    filtered.Add(issue);
                    issueAdded = true;
                  }
                }
              }
            }
          }
        }
      }

      DA.SetData(0, filtered.Count);
      DA.SetDataList(1, filtered);
    }

    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        //You can add image files to your project resources and access them like this:
        // return Resources.IconForThisComponent;
        return null;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("{9a02ad33-bd7f-42f9-ba82-bc4f237bfab6}"); }
    }
  }
}