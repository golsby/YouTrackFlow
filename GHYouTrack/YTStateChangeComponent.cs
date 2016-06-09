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
  public class YTStateChangeComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the YTStateChangeComponent class.
    /// </summary>
    public YTStateChangeComponent()
      : base("YTStateChangeComponent", "StateChange",
          "Count changes to YouTrack Issues by State field in time range",
          "YouTrack", "Summary")
    {
    }

    private Dictionary<string, string> _resolvedGroup = new Dictionary<string, string>()
    {
      {"Needs Doc", "Unresolved" },
      {"Needs Testing", "Unresolved" },
      {"Reopened", "Unresolved" },
      {"Submitted", "Unresolved" },
      {"Incomplete information", "Unresolved" },
      {"To be discussed", "Unresolved"},
      {"Awaiting Dependency", "Active"},
      {"In Progress", "Active"},
      {"Next Up", "Unresolved"},
      {"Open", "Unresolved"},
      {"Closed", "Resolved"},
      {"Can't Reproduce", "Ignored"},
      {"Inactive", "Ignored"},
      {"Duplicate", "Ignored"},
      {"Won't fix", "Ignored"},
      {"Obsolete", "Ignored"},
    };

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Issues", "i", "YouTrack Issues", GH_ParamAccess.list);
      pManager.AddTextParameter("States", "s", "States to report", GH_ParamAccess.list, "");
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("States", "s", "State Names", GH_ParamAccess.list);
      pManager.AddIntegerParameter("Values", "v", "Field Values", GH_ParamAccess.tree);
      pManager.AddTextParameter("Output", "out", "output", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      IssueCollection issues = new IssueCollection();
      List<string> states_to_display = new List<string>();
      DA.GetDataList(0, issues);
      DA.GetDataList("States", states_to_display);
      DateTime endTime = DateTime.Now, startTime = DateTime.Now;

      int days = 1;
      DA.GetData("WindowSize", ref days);

      List<string> keys = null;
      List<DateTime> timestamps = new List<DateTime>();
      DA.GetDataList(1, timestamps);

      DataTree<int> results = new DataTree<int>();

      // Each output branch represents a final state
      for (int t = 0; t < timestamps.Count; t++)
      {
        endTime = timestamps[t];
        startTime = endTime - new TimeSpan((int)days, 0, 0, 0);

        var summary = new StringCounter();
        foreach (var name in states_to_display)
          summary[name] = 0;

        foreach (var issue in issues)
        {
          List<Change> changes = issue.ChangesInRange(startTime, endTime);
          foreach (var change in changes)
          {
            if (change.Action == "state")
            {
              if (states_to_display.Contains(_resolvedGroup[change.To]))
                summary.Increment(_resolvedGroup[change.To]);
              if (states_to_display.Count == 0 || states_to_display.Contains(change.To))
              {
                summary.Increment(change.To);
                break;
              }
            }
          }
          if (issue.created > startTime && issue.created <= endTime)
          {
            summary.Increment("Created");
          }
        }

        StringBuilder output = new StringBuilder();
        keys = new List<string>();
        keys.AddRange(summary.Keys);
        keys.Sort();
        // GH_Path path = new GH_Path(t);
        for(int k=0; k < keys.Count; k++)
        {
          var key = keys[k];          
          results.Add(summary[key], new GH_Path(k));
          output.Append(key).Append(": ").Append(summary[key].ToString()).Append("\n");
        }
      }

      //DA.SetData(0, issues.Count);
      DA.SetDataList(1, keys);
      DA.SetDataTree(2, results);
      //DA.SetData(3, output.ToString());
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
      get { return new Guid("{5f1826c2-f674-4d87-8269-45ae83c5b01a}"); }
    }
  }
}