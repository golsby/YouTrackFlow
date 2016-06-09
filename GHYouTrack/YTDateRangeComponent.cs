using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHYouTrack
{
  public class YTDateRangeComponent : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the YTDateRangeComponent class.
    /// </summary>
    public YTDateRangeComponent()
      : base("YTDateRangeComponent", "Date Range",
          "Create a range of dates",
          "YouTrack", "Misc")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddTimeParameter("Date", "d", "Reference Date for range", GH_ParamAccess.item, DateTime.Now.Date);
      pManager.AddIntegerParameter("Days", "n", "Number of days in range", GH_ParamAccess.item, -30);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddTimeParameter("Dates", "d", "Dates", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      DateTime endDate = DateTime.MinValue;
      int days = 1;
      DA.GetData("Date", ref endDate);
      DA.GetData("Days", ref days);

      List<DateTime> results = new List<DateTime>();
      if (days > 0)
      {
        for (int i = 0; i < days; i++)
        {
          results.Add(endDate + new TimeSpan(i, 0, 0, 0));
        }
      }
      else
      {
        for (int i = 0; i > days; i--)
        {
          results.Add(endDate + new TimeSpan(i, 0, 0, 0));
        }
      }

      DA.SetDataList(0, results);
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
      get { return new Guid("{ed45ede0-ee11-4909-9d99-956e2445aedc}"); }
    }
  }
}