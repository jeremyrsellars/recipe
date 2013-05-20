using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections;

namespace ConsoleApplication1
{
   class Program
   {
      static void Main(string[] args)
      {
         Main2 (args);
         return;
         args = new [] 
         {
@"C:\code\jeremy.sellars\sensorsetup\SensorSetupApp.cs",
@"C:\code\jeremy.sellars\sensorsetupworking\SensorSetupApp.cs",
         };
         MainFiles (args);
         Console.ReadLine ();
      }
      static void MainFiles2(string[] args)
      {
         var f0 = System.IO.File.ReadAllText (args [0]);
         var f1 = System.IO.File.ReadAllText (args [1]);

         var sequence = new Sellars.Analysis.SimpleSequenceComparer<char> ().CompareSequences(f0,f1);
         WriteSequence (sequence, ConsoleColor.Cyan, ConsoleColor.Green);
      }
      static void MainFiles(string[] args)
      {
         var f0 = System.IO.File.ReadAllText (args [0]);
         var f1 = System.IO.File.ReadAllText (args [1]);

         var sequence = new Sellars.Analysis.SimpleSequenceComparer<char> ().CompareSequences(f0,f1);
         WriteSequence (sequence, ConsoleColor.Cyan, ConsoleColor.Green);
      }

      private static void WriteSequence<T>(IEnumerable<Sellars.Analysis.ISequencePoint<T>> sequence, ConsoleColor left, ConsoleColor right)
      {
         ConsoleColor normal = Console.ForegroundColor;
         foreach (Sellars.Analysis.ISequencePoint<T> sp in sequence)
         {
            switch (sp.Result)
            {
            case Sellars.Analysis.ComparisonResult.Left:
               Console.ForegroundColor = left;
               Console.Write (sp.Left);
               break;
            case Sellars.Analysis.ComparisonResult.Right:
               Console.ForegroundColor = right;
               Console.Write (sp.Right);
               break;
            case Sellars.Analysis.ComparisonResult.Both:
            default:
               Console.ForegroundColor = normal;
               Console.Write (sp.Left);
               break;
            }
         }
         Console.ForegroundColor = normal;
      }
      
      static void Main2(string[] args)
      {
         TestBitArrayND ();
         Dictionary<string,string> dict = new Dictionary<string,string> ();
         dict.Add  ("jeremy is cool", "Jeremy is the coolest!");

         dict.Add  (
@"jeremy is coolabcdefghijklmnopqrstuvwxyz", 
@"Jeremy is 
abcdefghijklmnopqrstuvwxyz
the coolest!");


         dict.Add  (
            "         for (int i = 0; i < dimensions [0]; i++)",
      "               for (int k = 0; k < dimensions [2]; k++)");

         //foreach (var sample in dict)
         //foreach (var x in new Sellars.Analysis.SimpleSequenceComparer<char> ().CompareSequences(sample.Key,sample.Value))
         //{
         //   Write (x);
         //}
         foreach (var sample in dict)
         {
            Console.WriteLine ("---------------------------------------------------------");
            Console.WriteLine ("Differences between these strings:\r\n1.  {0}\r\n2.  {1}", sample.Key, sample.Value);
            var sequence = new Sellars.Analysis.SimpleSequenceComparer<char> ().CompareSequences(sample.Key,sample.Value).ToList ();
            WriteSequence (sequence, Sellars.Analysis.ComparisonResult.Left, true, ConsoleColor.Cyan);
            Console.WriteLine ();
            WriteSequence (sequence, Sellars.Analysis.ComparisonResult.Right, true, ConsoleColor.Green);
            Console.WriteLine ();
         }
         
         Console.WriteLine ("Press enter");
         Console.ReadLine ();
      }

      private static void 
      WriteSequence(IEnumerable<Sellars.Analysis.ISequencePoint<char>> sequence, Sellars.Analysis.ComparisonResult comparisonResult, bool preserveSpacing, ConsoleColor differentColor)
      {
         ConsoleColor normal = Console.ForegroundColor;
         foreach (Sellars.Analysis.ISequencePoint<char> sp in sequence)
         {
            if ((sp.Result & comparisonResult) == comparisonResult)
            {
               if (sp.Result == Sellars.Analysis.ComparisonResult.Both)
                  Console.ForegroundColor = normal;
               else
                  Console.ForegroundColor = differentColor;
               char toPrint = (comparisonResult == Sellars.Analysis.ComparisonResult.Left ? sp.Left : sp.Right);
               if (toPrint == '\0')
               {
                  if (!preserveSpacing)
                     continue;
                  toPrint = ' ';
               }
               Console.Write (toPrint);
            }
         }
         Console.ForegroundColor = normal;
      }

      private static void Write(Sellars.Analysis.ISequencePoint<char> x)
      {
         ConsoleColor normal = Console.ForegroundColor;
         ConsoleColor left;
         ConsoleColor right;
         if (x.Result == Sellars.Analysis.ComparisonResult.Both)
         {
            left = normal;
            right = normal;
         }
         else
         {
            left = ConsoleColor.Cyan;
            right = ConsoleColor.Green;
         }
         Console.ForegroundColor = left;
         Console.Write (x.Left);
         Console.ForegroundColor = normal;
         Console.Write (' ');
         Console.ForegroundColor = right;
         Console.Write (x.Right);
         Console.ForegroundColor = normal;
         Console.WriteLine ();
      }
      
      static void TestBitArrayND ()
      {
         int [] dimensions = new [] {2,3,4,5,6};
         
         BitArrayND a = new BitArrayND (dimensions);
         
         a.Clear ();
         int expectedIndex = 0;
         for (int i = 0; i < dimensions [0]; i++)
         {
            for (int j = 0; j < dimensions [1]; j++)
            {
               for (int k = 0; k < dimensions [2]; k++)
               {
                  for (int l = 0; l < dimensions [3]; l++)
                  {
                     for (int m = 0; m < dimensions [4]; m++)
                     {
                        int [] index = new [] {i,j,k,l,m};
                        int index1d = a.GetIndex (index);
                        if (expectedIndex != index1d)
                        {
                           throw new InvalidOperationException ("Assertion failed: expected different index: " + index1d + ", actual: " + index1d);
                        }
                        if (a [index])
                        {
                           throw new InvalidOperationException (
                              index.Aggregate (
                                 new StringBuilder ("Assertion failed: "), 
                                 (s, n) => s.Append (n + ", "), 
                                 s => {s.Length-=2;return s.ToString ();}));
                        }
                        a [i,j,k,l,m] = true;
                        expectedIndex++;
                     }
                  }
               }
            }
         }

         return;
      }
   }
}
