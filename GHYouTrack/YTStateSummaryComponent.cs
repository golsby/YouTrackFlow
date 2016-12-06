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
  public class YTStateSummaryComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the YTStateChangeComponent class.
    /// </summary>
    public YTStateSummaryComponent()
      : base("YTStateSummaryComponent", "StateSummary",
          "Summarize YouTrack issues by state field",
          "YouTrack", "Summary")
    {
    }

    private Dictionary<string, string> _resolvedGroup = new Dictionary<string, string>()
    {
      {"Investigate", "Unresolved" },
      {"Unset", "Unresolved" },
      {"Needs Doc", "Unresolved" },
      {"Needs Testing", "Unresolved" },
      {"Reopened", "Unresolved" },
      {"Submitted", "Unresolved" },
      {"Incomplete information", "Unresolved" },
      {"To be discussed", "Unresolved"},
      {"Awaiting Dependency", "Unresolved"},
      {"In Progress", "Unresolved"},
      {"Next Up", "Unresolved"},
      {"Open", "Unresolved"},
      {"Closed", "Resolved"},
      {"Announce", "Resolved" },
      {"Can't Reproduce", "Resolved"},
      {"Inactive", "Resolved"},
      {"Duplicate", "Resolved"},
      {"Won't fix", "Resolved"},
      {"Obsolete", "Resolved"},
    };

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Issues", "i", "YouTrack Issues", GH_ParamAccess.tree);
      pManager.AddTextParameter("States", "s", "States to report", GH_ParamAccess.list, "");
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("States", "s", "State Names", GH_ParamAccess.list);
      pManager.AddIntegerParameter("Values", "v", "Field Values", GH_ParamAccess.tree);
      pManager.AddTextParameter("CSV", "csv", "CSV Output", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      GH_Structure<IGH_Goo> issue_tree = new GH_Structure<IGH_Goo>();
      List<string> states_to_display = new List<string>();
      DA.GetDataList("States", states_to_display);
      DA.GetDataTree("Issues", out issue_tree);
      states_to_display.Sort();

      StringBuilder csv = new StringBuilder();
      DataTree<int> results = new DataTree<int>();

      foreach (var state in states_to_display)
        csv.Append("\"").Append(state).Append("\",");
      csv.Append("\n");

      for (int b = 0; b < issue_tree.Branches.Count; b++)
      {
        var summary = new StringCounter();
        foreach (var state in states_to_display)
          summary[state] = 0;

        foreach (var gh_issue in issue_tree[b])
        {
          Issue issue = ((GH_ObjectWrapper)gh_issue).Value as Issue;
          if (issue == null)
            continue;

          var state = issue.GetPropertyValue("State");
          if (states_to_display.Contains(_resolvedGroup[state]))
          {
            summary.Increment(_resolvedGroup[state]);
          }
          if (states_to_display.Count == 0 || states_to_display.Contains(state))
          {
            summary.Increment(state);
            break;
          }
        }

        // Set ouptut data
        for (int k=0; k<states_to_display.Count; k++)
        {
          var state = states_to_display[k];
          results.Add(summary[state], new GH_Path(k));
          csv.Append(summary[state]).Append(",");
        }
        if (b < issue_tree.Branches.Count - 1)
          csv.Append("\n");
      }

      DA.SetDataList(0, states_to_display);
      DA.SetDataTree(1, results);
      DA.SetData(2, csv.ToString());
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
      get { return new Guid("{feaa7e0f-0e60-421d-9ee9-04217c71d609}"); }
    }
  }
}