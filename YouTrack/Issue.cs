using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;

namespace YouTrack
{
  [Serializable]
  public class Issue
  {
    public string id = "";
    public string entityId = "";
    public string summary = "";
    public string description = "";
    public DateTime created;
    public DateTime updated;
    public string updaterName = "";
    public string updaterFullName = "";
    public string reporterName = "";
    public string reporterFullName = "";
    public int commentsCount = 0;
    public int votes = 0;
    public string state = "";
    public string releaseTarget = "";
    public string priority = "";
    public string subsystem = "";
    public string assignee = "";
    public string type = "";
    public string publicView = "";

    public List<Change> changes = null;

    static DateTime FromJavaTimestamp(string JavaTime)
    {
      double dblJavaTime = 0.0;
      if (double.TryParse(JavaTime, out dblJavaTime))
      {
        var result = new DateTime(1970, 1, 1);
        return result.AddMilliseconds(dblJavaTime);
      }
      return DateTime.MinValue;
    }

    public Issue()
    { }

    public Issue(Issue src)
    {
      Copy(src);
    }

    public void Copy(Issue src)
    {
      id = src.id;
      entityId = src.entityId;
      summary = src.summary;
      description = src.description;
      created = src.created;
      updated = src.updated;
      updaterName = src.updaterName;
      updaterFullName = src.updaterFullName;
      reporterName = src.reporterName;
      reporterFullName = src.reporterFullName;
      commentsCount = src.commentsCount;
      votes = src.votes;
      state = src.state;
      releaseTarget = src.releaseTarget;
      priority = src.priority;
      subsystem = src.subsystem;
      assignee = src.assignee;
      type = src.type;
      publicView = src.publicView;
      changes = src.changes;
    }

    public static Issue GetHistory(Issue issue)
    {
      var q = new YouTrack.Query();
      var history = q.IssueHistory(issue.id);
      issue.SetChangesFromHistory(history);
      issue.SaveToTemp();
      // Console.WriteLine("{0} history fetched", issue.id);
      return issue;
    }

    public Issue AsOf(DateTime Timestamp)
    {
      if (this.created > Timestamp)
        return null;

      Issue result = new Issue(this);
      if (changes == null)
        return result;

      foreach (Change change in changes)
      {
        if (change.Timestamp >= Timestamp)
        {
          switch (change.Action)
          {
            case "releaseTarget":
              result.releaseTarget = change.From;
              break;
            case "assignee":
              result.assignee = change.From;
              break;
            case "state":
              result.state = change.From;
              break;
            case "priority":
              result.priority = change.From;
              break;
            case "type":
              result.type = change.From;
              break;
          }
        }
      }
      return result;
    }

    public List<Change> ChangesInRange(DateTime start, DateTime end)
    {
      var results = new List<Change>();

      foreach(var c in changes)
      {
        if (c.Timestamp > start && c.Timestamp <= end)
          results.Add(c);
      }

      return results;
    }

    public string GetPropertyValue(string propertyName)
    {
      var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
      foreach (var field in fields)
      {
        if (0 == string.Compare(field.Name, propertyName, true))
        {
          return field.GetValue(this) as string;
        }
      }
      return null;
    }


    public static Issue FromJObject(JToken src)
    {
      var result = new Issue();
      result.id = src["id"].Value<string>();
      result.entityId = src["entityId"].Value<string>();
      foreach(var field in src["field"])
      {
        var value = field["value"];
        if (field["name"].Value<string>() == "summary")
          result.summary = field["value"].Value<string>();
        if (field["name"].Value<string>() == "description")
          result.description = field["value"].Value<string>();
        if (field["name"].Value<string>() == "created")
          result.created = Issue.FromJavaTimestamp(field["value"].Value<string>());
        if (field["name"].Value<string>() == "updated")
          result.updated = Issue.FromJavaTimestamp(value.Value<string>());
        if (field["name"].Value<string>() == "updaterName")
          result.updaterName = field["value"].Value<string>();
        if (field["name"].Value<string>() == "updaterFullName")
          result.updaterFullName = field["value"].Value<string>();
        if (field["name"].Value<string>() == "reporterName")
          result.reporterName = field["value"].Value<string>();
        if (field["name"].Value<string>() == "reporterFullName")
          result.reporterFullName = field["value"].Value<string>();
        if (field["name"].Value<string>() == "commentsCount")
          result.commentsCount = field["value"].Value<int>();
        if (field["name"].Value<string>() == "votes")
          result.votes = field["value"].Value<int>();
        if (field["name"].Value<string>() == "Release target")
          result.releaseTarget = field["value"][0].Value<string>();
        if (field["name"].Value<string>() == "State")
          result.state = field["value"][0].Value<string>();
        if (field["name"].Value<string>() == "Priority")
          result.priority = field["value"][0].Value<string>();
        if (field["name"].Value<string>() == "Subsystem")
          result.subsystem = field["value"][0].Value<string>();
        if (field["name"].Value<string>() == "Assignee")
          result.assignee = field["value"][0]["fullName"].Value<string>();
        if (field["name"].Value<string>() == "Type")
          result.type = field["value"][0].Value<string>();
        if (field["name"].Value<string>() == "Public View")
          result.publicView = field["value"][0].Value<string>();
      }
      return result;
    }

    public List<Change> Diff(Issue other)
    {
      List<Change> changes = new List<Change>();
      Issue first, second;
      if (this.updated > other.updated)
      {
        first = other;
        second = this;
      }
      else
      {
        first = this;
        second = other;
      }
      if (first.releaseTarget != second.releaseTarget)
        changes.Add(new Change("releaseTarget", first.releaseTarget, second.releaseTarget, second.updated));
      if (first.assignee != second.assignee)
        changes.Add(new Change("assignee", first.assignee, second.assignee, second.updated));
      if (first.state != second.state)
        changes.Add(new Change("state", first.state, second.state, second.updated));
      if (first.priority != second.priority)
        changes.Add(new Change("priority", first.priority, second.priority, second.updated));
      if (first.type != second.type)
        changes.Add(new Change("type", first.type, second.type, second.updated));

      return changes;
    }

    public void SetChangesFromHistory(IssueCollection history)
    {
      changes = new List<Change>();
      for(int i=1; i<history.Count; i++)
      {
        changes.AddRange(history[i - 1].Diff(history[i]));
      }
    }

    static string GetTempDir()
    {
      string tempdir = Path.GetTempPath();
      string folder = Path.Combine(tempdir, "youtrack");
      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);
      return folder;
    }

    public void SaveToTemp()
    {
      string filename = Path.Combine(GetTempDir(), this.id + ".bin");
      Save(filename);
    }
    public void Save(string path)
    {
      IFormatter formatter = new BinaryFormatter();
      Stream stream = new FileStream(path,
                               FileMode.Create,
                               FileAccess.Write, FileShare.None);
      formatter.Serialize(stream, this);
      stream.Close();
    }

    public static Issue LoadFromTemp(string issueId)
    {
      string filename = Path.Combine(GetTempDir(), issueId + ".bin");
      if (File.Exists(filename))
        return Load(filename);
      return null;
    }

    public static Issue Load(string path)
    {
      IFormatter formatter = new BinaryFormatter();
      Stream stream = new FileStream(path,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read);
      Issue obj = (Issue)formatter.Deserialize(stream);
      stream.Close();
      return obj;
    }
  }
}
