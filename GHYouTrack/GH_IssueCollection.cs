using System;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using YouTrack;

namespace GHYouTrack
{
  class GH_Issue : GH_Goo<Issue>
  {
    public override bool IsValid
    {
      get
      {
        return true;
      }
    }

    public override string TypeDescription
    {
      get
      {
        return "GH_Issue";
      }
    }

    public override string TypeName
    {
      get
      {
        return "GH_Issue";
      }
    }

    public override IGH_Goo Duplicate()
    {
      var dup = new GH_Issue();
      dup.m_value = new Issue(m_value);
      return dup;
    }

    public override string ToString()
    {
      return string.Format(m_value.ToString());
    }
  }

  class GH_IssueCollection : GH_Goo<IssueCollection>
  {
    public override bool IsValid
    {
      get
      {
        return true;
      }
    }

    public override string TypeDescription
    {
      get
      {
        return "IssueCollection";
      }
    }

    public override string TypeName
    {
      get
      {
        return "GH_IssueCollection";
      }
    }

    public override IGH_Goo Duplicate()
    {
      GH_IssueCollection copy = new GH_IssueCollection();
      foreach (var issue in this.m_value)
        copy.m_value.Add(issue);
      return copy;
    }

    public override string ToString()
    {
      return "GH_IssueCollection.ToString()";
    }

    public IssueCollection IssueCollection
    {
      get { return m_value; }
      set { m_value = value; }
    }
  }
}
