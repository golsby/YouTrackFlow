using System;

namespace YouTrack
{
  [Serializable]
  public class Change
  {
    public string Action;
    public string From;
    public string To;
    public DateTime Timestamp;

    public Change(string Action, string From, string To, DateTime Timestamp)
    {
      this.Action = Action;
      this.From = string.IsNullOrWhiteSpace(From) ? "Unset" : From;
      this.To = string.IsNullOrWhiteSpace(To) ? "Unset" : To;
      this.Timestamp = Timestamp;
    }

    public override string ToString()
    {
      return string.Format("{0}: {1} -> {2} ({3})", Action, From, To, Timestamp.ToShortDateString());
    }
  }
}
