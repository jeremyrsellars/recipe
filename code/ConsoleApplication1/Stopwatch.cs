using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Analysis
{
   public struct Stopwatch : IDisposable
   {
      public
      Stopwatch (string text)
      {
         Text = text;
         Start = DateTime.UtcNow;
         End = new DateTime ();
      }
      public DateTime Start;
      public DateTime End;
      public string Text;
      public void Dispose ()
      {
         End = DateTime.UtcNow;
         Console.Error.WriteLine (Text + " " + (End - Start).TotalMilliseconds + "ms");
      }
   }
}
