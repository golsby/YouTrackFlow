using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using YouTrack;
using YouTrackSharp.Infrastructure;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using JsonFx.IO;
using System.IO;
using System.Security.Cryptography;

namespace YouTrack
{
  public class Query
  {
    public string Project { get; set; }
    public DateTime CreatedAfter { get; set; }
    public DateTime CreatedBefore { get; set; }
    public DateTime UpdatedAfter { get; set; }
    public DateTime UpdatedBefore { get; set; }
    public string Filter { get; set; }
    public string ReleaseTarget { get; set; }
    NetworkCredential _credentials = null;
    static CookieCollection _authenticationCookies = null;

    public NetworkCredential Credentials
    {
      set
      {
        _credentials = value;
        PromptForCredentials = false;
      }
    }

    public bool PromptForCredentials { get; set; }

    public Query()
    {
      PromptForCredentials = true;
    }

    public IssueCollection FetchAllIssues()
    {
      var collection = new IssueCollection();
      int batchSize = 100;
      int start = 0;
      List<string> fields = new List<string>()
      {
        "id", "updated"
      };

      DateTime lastUpdated = DateTime.MinValue;
      var cache = QueryCache.LoadFromTemp(this.Filter.ToString());
      if (false && cache != null)
      {
        if (cache.Filter.ToUpperInvariant() == this.Filter.ToString().ToUpperInvariant())
        {
          foreach (LightIssue issue in cache.Issues)
          {
            collection.Add(new Issue() { id = issue.id, updated=issue.updated });
          }
          this.UpdatedAfter = cache.LastUpdated;
          cache.LastUpdated = DateTime.Now.Date;
        }
      }

      var received = FetchIssues(batchSize, start);
      collection.AddRange(received);
      while (received.Count == batchSize)
      {
        start += received.Count;
        received = FetchIssues(batchSize, start);
        collection.AddRange(received);
      }

      var issues_with_history = new IssueCollection();

      Parallel.ForEach(collection, (issue) =>
      {
        var cached_issue = Issue.LoadFromTemp(issue.id);
        if (false && cached_issue != null && (cached_issue.updated <= issue.updated || issue.updated == DateTime.MinValue))
        {
          // Console.WriteLine("{0} loaded from cache", issue.id);
          issues_with_history.Add(cached_issue);
          return;
        }
        issues_with_history.Add(Issue.GetHistory(issue));
        System.Diagnostics.Trace.WriteLine(string.Format("Loading history for issue {0}", issue.id));
      });

      cache.Issues.Clear();
      cache.AddIssues(issues_with_history);
      cache.SaveToTemp();

      return issues_with_history;
    }
    public void AskForCredentials(Func<string, string, CookieCollection> authenticate)
    {
      string name = "";
      var dialog = new SecureCredentialsLibrary.CredentialsDialog("GH_YouTrack");
      if (name != null) dialog.AlwaysDisplay = true; // prevent an infinite loop
      if (dialog.Show(name) == DialogResult.OK)
      {
        _authenticationCookies = authenticate(dialog.Name, dialog.Password);
      }
    }

    void Authenticate(Connection connection)
    {
      if (_authenticationCookies != null)
      {
        connection.AuthenticationCookies = _authenticationCookies;
        return;
      }
      if (PromptForCredentials)
        AskForCredentials(connection.GetAuthenticationCookies);
      else
        connection.Authenticate(_credentials);
    }


    public IssueCollection IssueHistory(string IssueId)
    {
      var collection = new IssueCollection();
      collection.AllowDuplicates = true;
      var client = new WebClient();
      var connection = new Connection("mcneel.myjetbrains.com", 80, false, "youtrack");
      Authenticate(connection);

      var path = string.Format("issue/{0}/history", IssueId);

      var request = connection.CreateHttpRequest();
      var command = connection._uriConstructor.ConstructBaseUri(path);
      var response = request.Get(command);

      var dyn = Newtonsoft.Json.Linq.JArray.Parse(response.RawText);

      foreach (var i in dyn)
      {
        collection.Add(Issue.FromJObject(i));
      }

      return collection;
    }

    public IssueCollection FetchIssues(int max = 10, int start = 0, IEnumerable<string> returnFields = null)
    {
      var collection = new IssueCollection();
      var connection = new Connection("mcneel.myjetbrains.com", 80, false, "youtrack");
      Authenticate(connection);

      var uri = new Uri("http://mcneel.myjetbrains.com/issue");

      uri = uri.AddQuery("filter", this.ToString());
      uri = uri.AddQuery("max", max.ToString());
      uri = uri.AddQuery("after", start.ToString());
      if (returnFields != null)
      {
        foreach (var field in returnFields)
        {
          uri = uri.AddQuery("with", field);
        }
      }

      var request = connection.CreateHttpRequest();
      var command = connection._uriConstructor.ConstructBaseUri(uri.PathAndQuery.TrimStart("/".ToCharArray()));
      var response = request.Get(command);

      var dyn = Newtonsoft.Json.Linq.JObject.Parse(response.RawText);

      foreach (var d in dyn)
      {
        foreach (var i in d.Value)
        {
          collection.Add(Issue.FromJObject(i));
        }
      }

      return collection;
    }

    static string DateRange(DateTime start, DateTime end)
    {
      if (start == DateTime.MinValue)
        start = new DateTime(1970, 1, 1);
      if (end == DateTime.MinValue)
        end = DateTime.Now.Date + new TimeSpan(24, 0, 0);

      StringBuilder result = new StringBuilder();
      result.Append(start.ToString("yyyy-MM-ddTHH:mm"));
      result.Append(" .. ");
      result.Append(end.ToString("yyyy-MM-ddTHH:mm"));

      return result.ToString();
    }

    public override string ToString()
    {
      var result = new StringBuilder();
      if (!string.IsNullOrEmpty(Project))
      {
        if (result.Length > 0) result.Append(" AND");
        result.Append(" project: ").Append(Project);
      }
      if (!string.IsNullOrEmpty(ReleaseTarget))
      {
        if (result.Length > 0) result.Append(" AND");
        result.Append(" release target: ").Append(ReleaseTarget);
      }
      if (UpdatedAfter != DateTime.MinValue || UpdatedBefore != DateTime.MinValue)
      {
        if (result.Length > 0) result.Append(" AND");
        result.Append(" updated: ").Append(DateRange(UpdatedAfter, UpdatedBefore));
      }
      if (CreatedAfter != DateTime.MinValue || CreatedBefore != DateTime.MinValue)
      {
        if (result.Length > 0) result.Append(" AND");
        result.Append(" created: ").Append(DateRange(CreatedAfter, CreatedBefore));
      }
      if (!string.IsNullOrEmpty(Filter))
      {
        if (result.Length > 0) result.Append(" AND");
        result.Append(" (").Append(Filter).Append(")");
      }

      return result.ToString();
    }
  }

  [Serializable]
  public class LightIssue
  {
    public string id { get; set; }
    public DateTime updated { get; set; }
    public static LightIssue FromIssue(Issue i)
    {
      var rc = new LightIssue();
      rc.id = i.id;
      rc.updated = i.updated;
      return rc;
    }

    public override string ToString()
    {
      return string.Format("{0} updated {1}", this.id, this.updated);
    }
  }

  [Serializable]
  public class QueryCache
  {
    public string Filter { get; set; }
    public List<LightIssue> Issues = new List<LightIssue>();
    public DateTime LastUpdated = DateTime.MinValue;
    public void AddIssues(IEnumerable<Issue> issues)
    {
      foreach (var issue in issues)
      {
        Issues.Add(LightIssue.FromIssue(issue));
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

    static string TempFileName(string filter)
    {
      using (var sha1 = new SHA1Managed())
      {
        string lc_filter = filter.ToLowerInvariant();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(lc_filter));
        var sb = new StringBuilder(hash.Length * 2);

        foreach (byte b in hash)
        {
          // can be "x2" if you want lowercase
          sb.Append(b.ToString("X2"));
        }

        return "_Query_" + sb.ToString() + ".bin";
      }
    }

    string TempFileName()
    {
      return TempFileName(Filter);
    }

    public void SaveToTemp()
    {
      string filename = Path.Combine(GetTempDir(), TempFileName());
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

    public static QueryCache LoadFromTemp(string Filter)
    {
      string filename = Path.Combine(GetTempDir(), TempFileName(Filter));
      if (File.Exists(filename))
        return Load(filename);
      var result = new QueryCache();
      result.Filter = Filter;
      return result;
    }

    public static QueryCache Load(string path)
    {
      try
      {
        IFormatter formatter = new BinaryFormatter();
        using (var stream = new FileStream(path,
                                  FileMode.Open,
                                  FileAccess.Read,
                                  FileShare.Read))
        {
          QueryCache obj = (QueryCache)formatter.Deserialize(stream);
          return obj;
        }
      }
      catch
      {
        System.IO.File.Delete(path);
        return null;
      }
    }
  }
}
