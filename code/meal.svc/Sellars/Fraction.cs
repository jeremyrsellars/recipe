using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars
{
   /// <summary>
   /// Represents a fractional number.
   /// </summary>
   [Serializable]
   public struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
   {
      public
      Fraction (int wholePart)
         : this (wholePart < 0, System.Math.Abs (wholePart), 0, 1)
      {
      }
      
      public
      Fraction (int numerator, int denominator)
         : this (numerator < 0, 0, Math.Abs (numerator), denominator)
      {
      }
      
      public 
      Fraction (int wholePart, int numerator, int denominator)
         : this (wholePart < 0 || numerator < 0, Math.Abs (wholePart), Math.Abs (numerator), denominator)
      {
         if ((wholePart > 0 && numerator < 0)
          || (wholePart < 0 && numerator > 0)) 
         {
            throw new ArgumentException ("ambiguous negativity because signs of wholePart and numerator don't match.");
         }
      }
      
      public static readonly Fraction Zero;
      
      public bool Negative
      {
         get
         {
            return m_negative;
         }
      }
      
      public int WholePart
      {
         get
         {
            return m_whole;
         }
      }
      
      public int Numerator
      {
         get
         {
            return m_numer;
         }
      }
      
      public int Denominator
      {
         get
         {
            return Less1ToInt32 (m_denomLess1);
         }
      }
      
      /// <summary>
      /// Returns an equivilent <cref>Fraction</cref> (by moving excess numerator to the whole part).
      /// </summary>
      /// <returns>An equivilant fraction.</returns>
      /// <example>Converts "3/2" to "1 1/2".</example>
      /// <remarks>ExampleConverts .</remarks>
      public Fraction Normalize ()
      {
         Fraction f = Normalize (Denominator);

         int num = f.Numerator;
         int denom = f.Denominator;

         List<int> numFactors = PrimeFactorization (f.Numerator);
         List<int> denomFactors = PrimeFactorization (f.Denominator);

         for (int numIndex = 0; numIndex < numFactors.Count; numIndex++)
         {
            int factor = numFactors[numIndex];
            int denomIndex = denomFactors.IndexOf (factor);
            if (denomIndex < 0)
               continue;
            num /= factor;
            denom /= factor;
            denomFactors [denomIndex] = 1;
         }

         return new Fraction (f.WholePart, num, denom);
      }
      
      /// <summary>
      /// Returns a near-equivilent <cref>Fraction</cref> with a minimal numerator (by moving excess numerator to the whole part).
      /// </summary>
      /// <param name="denominator">The new denominator for the fraction.</param>
      /// <returns>A near equivilant fraction with the specified <paramref name="denominator"/>.</returns>
      /// <remarks>May result in loss of precision if <paramref name="denominator"/> is not a multiple of <cref>Denominator</cref>.</remarks>
      public Fraction Normalize (int denominator)
      {
         ValidateDenominator (denominator);
         
         int addWhole = Numerator / Denominator;
         return new Fraction (
            Negative,
            WholePart + addWhole,
            (Numerator - addWhole * Denominator) * denominator / Denominator,
            denominator);
      }
      
      private static uint Less1FromInt32 (int denominator)
      {
         ValidateDenominator(denominator);
         
         return ((uint) (denominator - 1));
      }

      private static void ValidateDenominator(int denominator)
      {
         if (denominator < 1)
            throw new ArgumentOutOfRangeException("denominator", denominator, "denominator must be greater than zero.");
      }
      
      private static int Less1ToInt32 (uint denomLess1)
      {
         if (denomLess1 < 0)
            throw new ArgumentOutOfRangeException ("denomLess1", denomLess1, "denominator must be greater than zero.");
         
         return ((int) (denomLess1 + 1));
      }

      public override string ToString()
      {
         StringBuilder s = new StringBuilder (32);
         
         if (Negative)
            s.Append ('-');
         
         bool showFraction = Numerator != 0;
         bool showWhole = !showFraction || WholePart != 0;
         
         if (showWhole)
            s.Append (WholePart);
         if (showWhole && showFraction)
            s.Append (' ');
         if (showFraction)
         {
            s.Append (Numerator);
            s.Append ('/');
            s.Append (Denominator);
         }
         
         return s.ToString ();
      }
      
      public static Fraction Parse (string s)
      {
         Fraction result;
         if (!TryParse (s, out result))
            throw new FormatException ("Could not parse fraction: " + s);
         
         return result;
      }
      
      public static bool 
      TryParse (string s, out Fraction result)
      {
         #warning Optimize Fraction.TryParse
         if (s == null)
         {
            result = new Fraction ();
            return false;
         }
         int slash = s.IndexOf ('/');
         
         if (slash < 0)
         {
            int value;
            if (int.TryParse (s, out value))
            {
               result = new Fraction (value);
               return true;
            }
            result = new Fraction ();
            return false;
         }
         if (slash == 0)
         {
            result = new Fraction ();
            return false;
         }
         
         try
         {
            string [] before = s.Substring (0, slash).Split (new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
            int whole;
            int numer;
            int denom;
            if (before.Length == 2)
            {
               whole = int.Parse (before [0]);
               numer = int.Parse (before [1]);
            }
            else if (before.Length == 1)
            {
               whole = 0;
               numer = int.Parse (before [0]);
            }
            else
            {
               result = new Fraction ();
               return false;
            }
            
            denom = int.Parse (s.Substring (slash + 1));
            
            result = new Fraction(whole, numer, denom);
            return true;
         }
         catch (Exception)
         {
            result = new Fraction ();
            return false;
         }
      }

      public override bool Equals(object obj)
      {
         if (obj is Fraction)
            return Equals ((Fraction) obj);
         return false;
      }

      public override int GetHashCode()
      {
         return ToDouble ().GetHashCode();
      }

      public static bool operator== (Fraction a, Fraction b)
      {
         return a.Equals (b);
      }

      public static bool operator!= (Fraction a, Fraction b)
      {
         return !a.Equals (b);
      }

      #region IEquatable<Fraction> Members

      public bool Equals(Fraction other)
      {
         Fraction a = this.Normalize ();
         Fraction b = other.Normalize ();
         
         return a.m_whole == b.m_whole
            && a.m_numer == b.m_numer
            && a.m_denomLess1 == b.m_denomLess1
            && a.m_negative == b.m_negative;
      }

      #endregion

      public Double ToDouble ()
      {
         Double value = (Double)m_whole + ((Double)m_numer / (Double)Denominator);
         value *= m_negative ? -1.0 : 1.0;
         return value;
      }

      #region IComparable<Fraction> Members

      public int CompareTo(Fraction other)
      {
         return ToDouble ().CompareTo (other.ToDouble ());
      }

      public static implicit operator Fraction (int value)
      {
         return new Fraction (value);
      }

      #endregion
      
      /// <summary>
      /// Creates a new Fraction.
      /// </summary>
      /// <param name="negative">A value indicating whether the fraction is less than zero,</param>
      /// <param name="wholePart">A positive number representing the whole part of the fraction.</param>
      /// <param name="numerator">A positive number representing the numerator of the fraction.</param>
      /// <param name="denominator">A positive number representing the denominator of the fraction.</param>
      private
      Fraction (bool negative, int wholePart, int numerator, int denominator)
      {
         m_denomLess1 = Less1FromInt32 (denominator);
         m_negative = negative;
         m_whole = wholePart;
         m_numer = numerator;
      }
      
      private List<int> PrimeFactorization (int number)
      {
         List<int> factors = new List<int> ();
         if (number < 0)
         {
            factors.Add (-1);
            number = -number;
         }
         if (number < 4)
         {
            factors.Add (number);
            return factors;
         }
         
         int unfactored = number;
         foreach (int prime in GetPrimes ())
         {
            int remainder;
            int result;
            
            result = Math.DivRem (unfactored, prime, out remainder);
            while (remainder == 0)
            {
               unfactored = result;
               factors.Add (prime);
               result = Math.DivRem (unfactored, prime, out remainder);
            }

            if (prime > unfactored)
            {
               if (unfactored == 1)
                  return factors;
               throw new InvalidOperationException ("Bad algorithm: prime > unfactored");
            }
         }
         return factors;
      }

      private static IEnumerable<int> GetPrimes ()
      {
         List<int> primes = new List<int> ();
         yield return 2;
         // primes.Add (2); // Don't really need to add it to our list because we've already returned it
         // and we'll never test even numbers.

         for (int primeCandidate = 3; primeCandidate <= int.MaxValue; primeCandidate += 2)
         {
            bool prime = true;
            for (int i = 0; i < primes.Count; i++)
            {
               int remainder = primeCandidate % primes[i];
               if (remainder == 0)
               {
                  prime = false;
                  break;
               }
            }
            if (prime)
            {
               yield return primeCandidate;
               primes.Add (primeCandidate);
            }
         }
      }

      private readonly bool m_negative;
      private readonly int m_whole;
      private readonly int m_numer;
      // The denominator of the fraction minus 1.
      private uint m_denomLess1;
   }
}
