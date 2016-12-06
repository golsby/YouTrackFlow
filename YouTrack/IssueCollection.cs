using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTrack
{
  public class IssueCollection : List<Issue>
  {
    public bool AllowDuplicates { get; set; }
    public IssueCollection()
    {
      AllowDuplicates = false;
    }
    public IssueCollection AsOf(DateTime Timestamp)
    {
      IssueCollection result = new IssueCollection();
      foreach(Issue issue in this)
      {
        if (issue == null)
          continue;
        var issueAsOf = issue.AsOf(Timestamp);
        if (issueAsOf != null)
          result.Add(issueAsOf);
      }
      return result;
    }

    public new void AddRange(IEnumerable<Issue> issues )
    {
      foreach (var issue in issues)
      {
        Add(issue);
      }
    }

    public new void Add(Issue issue)
    {
      if (!AllowDuplicates)
        base.Remove(issue);
      base.Add(issue);
    }

    public new void Clear()
    {
      base.Clear();
    }

    int CountState(string name)
    {
      int c = 0;
      foreach (var i in this)
        if (i.state == name)
          c += 1;
      return c;
    }

    public int NeedsDocCount
    {
      get
      {
        return CountState("Needs Doc");
      }
    }

    public int NeedsTestingCount
    {
      get
      {
        return CountState("Needs Testing");
      }
    }

    public int ReopenedCount
    {
      get
      {
        return CountState("Reopened");
      }
    }

    public int SubmittedCount
    {
      get
      {
        return CountState("Submitted");
      }
    }

    public int IncompleteCount
    {
      get
      {
        return CountState("Incomplete information");
      }
    }

    public int ToBeDiscussedCount
    {
      get
      {
        return CountState("To be discussed");
      }
    }

    public int AwaitingDependencyCount
    {
      get
      {
        return CountState("Awaiting Dependency");
      }
    }
    public int InProgressCount
    {
      get
      {
        return CountState("In Progress");
      }
    }
    public int NextUpCount
    {
      get
      {
        return CountState("Next Up");
      }
    }
    public int OpenCount
    {
      get
      {
        return CountState("Open");
      }
    }
    public int ClosedCount
    {
      get
      {
        return CountState("Closed");
      }
    }
    public int CantReproduceCount
    {
      get
      {
        return CountState("Can't Reproduce");
      }
    }
    public int InactiveCount
    {
      get
      {
        return CountState("Inactive");
      }
    }
    public int DuplicateCount
    {
      get
      {
        return CountState("Duplicate");
      }
    }
    public int WontFixCount
    {
      get
      {
        return CountState("Won't fix");
      }
    }
    public int ObsoleteCount
    {
      get
      {
        return CountState("Obsolete");
      }
    }

  }
}
