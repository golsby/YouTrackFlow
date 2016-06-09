using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using YouTrack;
using YouTrackSharp.Infrastructure;
using System.Windows.Forms;
using System.Threading.Tasks;

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
        if (cached_issue != null && cached_issue.updated <= issue.updated)
        {
          // Console.WriteLine("{0} loaded from cache", issue.id);
          issues_with_history.Add(cached_issue);
          return;
        }
        issues_with_history.Add(Issue.GetHistory(issue));
      });

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

    public IssueCollection FetchIssues(int max=10, int start=0, IEnumerable<string> returnFields=null)
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

      foreach(var d in dyn)
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
        end = DateTime.Now.Date + new TimeSpan(24,0,0);

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


}
