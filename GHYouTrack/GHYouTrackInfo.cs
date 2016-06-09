using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GHYouTrack
{
  public class GHYouTrackInfo : GH_AssemblyInfo
  {
    public override string Name
    {
      get
      {
        return "YouTrack";
      }
    }
    public override Bitmap Icon
    {
      get
      {
        //Return a 24x24 pixel bitmap to represent this GHA library.
        return null;
      }
    }
    public override string Description
    {
      get
      {
        //Return a short string describing the purpose of this GHA library.
        return "";
      }
    }
    public override Guid Id
    {
      get
      {
        return new Guid("7bc303a5-a367-4f46-ae6d-7214bae467a4");
      }
    }

    public override string AuthorName
    {
      get
      {
        //Return a string identifying you or your company.
        return "";
      }
    }
    public override string AuthorContact
    {
      get
      {
        //Return a string representing your preferred contact details.
        return "";
      }
    }
  }
}
