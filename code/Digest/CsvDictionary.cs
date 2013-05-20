using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Digest
{
   class CsvDictionary
   {
      public static Dictionary<string,string> FromFile (string file)
      {
         Dictionary<string,string> dict = new Dictionary<string,string> ();
         using (var rdr = System.IO.File.OpenText (file))
         {
            int lineNumber = 0;
            string line;
            int comma;

            while (null != (line = rdr.ReadLine ()))
            {
               if (lineNumber++ == 0)
                  continue;  // Skip header.
               string key, value;
               comma = line.IndexOf (',');
               if (comma <= 0)
                  continue;
               key = line.Substring (0, comma);
               value = line.Substring (comma + 1);
               dict.Add (key, value);
            }
         }
         return dict;
      }

      public static List<Dictionary<string,string>> ListFromFile (string file)
      {
         List<Dictionary<string,string>> list = new List<Dictionary<string,string>> ();
         Regex regex = new Regex (@"
         (?<=,|^)
         (
            (?<field>(([^,""\r\n]*("""")?))+)
            |
            ""(?<field>(([^,""\r\n]*(""""|,|\r\n|\n)?)+))""
         )
         (,|$|\r\n|\n)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

         Regex regexNewLine = new Regex (@"\r\n|\n");

         using (var rdr = System.IO.File.OpenText (file))
         {
            int lineNumber = 0;
            string line;
            int comma ;

            // Process header
            if (null == (line = rdr.ReadLine ()))
            {
               return null;
            }

            //List<string> fields = new List<string> ();
            //comma = -1;
            //for (;;)
            //{
            //   int fieldStart = comma + 1;
            //   comma = line.IndexOf (',', fieldStart);
            //   if (comma < 0)
            //      break;
            //   string field = line.Substring (fieldStart, comma - fieldStart);
            //   fields.Add (field);
            //}

            List<string> fields;
            
            fields = regex.Matches (line)
               .Cast<Match> ()
               .Select (Match => Match.Groups["field"].Value)
               .ToList ();

            // Process rows
            string all = rdr.ReadToEnd ();
            int fieldIndex = 0;
            Dictionary<string,string> dict = null;
            for (Match match = regex.Match (all); match.Success; match = match.NextMatch ())
            {
               int idx = fieldIndex++ % fields.Count;
               if (idx == 0)
               {
                  dict = new Dictionary<string,string> ();
                  list.Add (dict);
               }

               string field = fields [idx];
               string value = regexNewLine.Replace (match.Groups["field"].Value.Replace ("\"\"", "\""), Environment.NewLine);
               dict.Add (field, value);
            }

            if (dict.Count == 1 && dict.Values.First () == "")
               list.Remove (dict);

            //// Process rows
            //while (null != (line = rdr.ReadLine ()))
            //{
            //   List<string> values;
            //   line //.Where(c => !char.IsLetter(c) && c != ' ')
            //      .ToList ().ForEach (c => Console.WriteLine ("0x" + ((int)c).ToString ("X") + " '" + c + "' " + (int)c));
            //   Match m = regex.Match (line);
            //   MatchCollection x = regex.Matches (line);
            //   values = x
            //      .Cast<Match> ()
            //      .Select (match => match.Groups["field"].Value)
            //      .Select (s => s.Replace ("\"\"", "\""))  // Replace "" with ".
            //      .ToList ();

            //   if (fields.Count != values.Count)
            //   {
            //      throw new InvalidOperationException ("fields.Count != values.Count: " + fields.Count + " != " + values.Count);
            //   }

            //   Dictionary<string,string> dict = new Dictionary<string,string> ();
            //   for (int i = 0; i <fields.Count; i++)
            //   {
            //      dict.Add (fields[i], values[i]);
            //   }
            //   list.Add (dict);
            //}
         }
         return list;
      }
   }
}
