using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Model
{
   public class FractionRange : NotifyPropertyChangedObject
   {
      private static readonly char [] DASH = new [] {'-'};
      private string m_text;
      private Fraction m_amount;
      private Fraction m_amountMax;

      public FractionRange ()
         : this (new Fraction (), new Fraction ())
      {
      }

      public FractionRange (Fraction amount)
         : this (amount, new Fraction ())
      {
      }

      public FractionRange (Fraction amount, Fraction amountMax)
      {
         m_text = "";
         Amount = amount;
         AmountMax = amountMax;
      }

      public string Text
      {
         get
         {
            return m_text;
         }
         set
         {
            if (SetValue (ref m_text, value, "Text"))
            {
               Fraction amount;
               Fraction amountMax;
               string text = value.Trim ();
               int dash = text.IndexOf ('-');
               // Parse negatives as a single amount.
               if (dash <= 1)
               {
                  Fraction.TryParse (value, out amount);
                  amountMax = new Fraction();
               }
               else
               {
                  string [] split = text.Split (DASH, 2);
                  Fraction.TryParse (split[0].Trim (), out amount);
                  Fraction.TryParse (split[1].Trim (), out amountMax);
               }

               SetValues (amount, amountMax);
            }
         }
      }

      public Fraction Amount
      {
         get
         {
            return m_amount;
         }
         set
         {
            if (SetValue (ref m_amount, value, "Amount"))
               Text = ToString ();
         }
      }

      public Fraction AmountMax
      {
         get
         {
            return m_amountMax;
         }
         set
         {
            if (SetValue (ref m_amountMax, value, "AmountMax"))
               Text = ToString ();
         }
      }

      public override string ToString()
      {
         return ToString (Amount, AmountMax);
      }

      public static string ToString(Fraction amount, Fraction amountMax)
      {
         if (amountMax == new Fraction () && amount == new Fraction ())
            return "";
         if (amountMax == new Fraction ())
            return amount.ToString ();

         return amount + " - " + amountMax;
      }

      private void SetValues (Fraction amount, Fraction amountMax)
      {
         string toString = ToString (amount, amountMax);
         Fraction oldAmount = m_amount;
         Fraction oldAmountMax = m_amountMax;

         bool changedText = m_text != toString;
         bool changedAmount = oldAmount != amount;
         bool changedAmountMax = oldAmountMax != amountMax;

         m_text = toString;
         m_amount = amount;
         m_amountMax = amountMax;

         if (changedText)
            OnPropertyChanged ("Text");
         if (changedAmount)
            OnPropertyChanged ("Amount");
         if (changedAmountMax)
            OnPropertyChanged ("AmountMax");
      }
   }
}
