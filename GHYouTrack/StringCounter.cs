using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHYouTrack
{
  public class StringCounter : Dictionary<string, int>
  {
    public void Increment(string key)
    {
      if (!this.ContainsKey(key))
        this[key] = 1;
      else
        this[key] += 1;
    }

    public void Decrement(string key)
    {
      if (!this.ContainsKey(key))
        this[key] = -1;
      else
        this[key] -= 1;
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      return sb.ToString();
    }
  }
}
