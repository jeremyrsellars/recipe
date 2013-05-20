using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using Visibility=System.Windows.Visibility;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(string), typeof(TimeSpan))]
   public class CookTimespanConverter : IValueConverter
   {
      public CookTimespanConverter ()
      {
         DayFormat = "{0} day";
         DaysFormat = "{0} days";
         HourFormat = "{0} hour";
         HoursFormat = "{0} hours";
         MinuteFormat = "{0} minute";
         MinutesFormat = "{0} minutes";
         SecondFormat = "{0} second";
         SecondsFormat = "{0} seconds";
         DefaultUnit = TimeUnit.Minutes;
      }
      
      public string DayFormat{get;set;}
      public string DaysFormat{get;set;}
      public string HourFormat{get;set;}
      public string HoursFormat{get;set;}
      public string MinuteFormat{get;set;}
      public string MinutesFormat{get;set;}
      public string SecondFormat{get;set;}
      public string SecondsFormat{get;set;}
      public TimeUnit DefaultUnit{get;set;}
      
      public enum TimeUnit
      {
         Days,
         Hours,
         Minutes,
         Seconds,
      }
      
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is TimeSpan)
         {
            TimeSpan t = (TimeSpan)value;
            
            StringBuilder s = new StringBuilder ();
            Func<int,string,string,bool> append = 
               (time, unit, pluralUnit) =>
               {
                  if (time == 0)
                     return false;
                  if (s.Length > 0)
                     s.Append (' ');
                  if (time == 1)
                     s.AppendFormat (unit, time);
                  else
                     s.AppendFormat (pluralUnit, time);
                  return true;
               };
            if (append(t.Days, DayFormat, DaysFormat)
             | append(t.Hours, HourFormat, HoursFormat)
             | append(t.Minutes, MinuteFormat, MinutesFormat)
             | append(t.Seconds, SecondFormat, SecondsFormat))
            {
               return s.ToString ();
            }
            return null;
         }
         return null;
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is TimeSpan)
            return value;
         if (value is string)
         {
            TimeSpan ts;
            if (TryParseTimespan ((string)value, out ts))
               return ts;
         }
         return null;
      }
      
      public bool TryParseTimespan (string s, out TimeSpan ts)
      {
         if (s.Contains ('/'))
            return TryParseFractionTimespan (Tokenize (s, true), out ts);
         return TryParseDecimalTimespan (Tokenize (s, false), out ts);
      }
      
      private bool TryParseFractionTimespan (IEnumerable<Token> tokens, out TimeSpan ts)
      {
         ts = new TimeSpan ();
         bool expectingUnit = false;
         
         var enumerator = tokens.GetEnumerator ();
         double amount = 0.0;
         while (enumerator.MoveNext ())
         {
            Token token = enumerator.Current;
            if (token.Type == TokenType.WhiteSpace)
               continue;
            if (token.Type == TokenType.Character && expectingUnit)
            {
               char first=token.Value [0];
               expectingUnit = false;
               switch (first)
               {
               case 'd':
               case 'D':
                  ts += TimeSpan.FromDays (amount);
                  break;
               case 'h':
               case 'H':
                  ts += TimeSpan.FromHours (amount);
                  break;
               case 'm':
               case 'M':
                  ts += TimeSpan.FromMinutes (amount);
                  break;
               case 's':
               case 'S':
                  ts += TimeSpan.FromSeconds (amount);
                  break;
               default:
                  return false;
               }
            }
            else if (token.Type == TokenType.Number || token.Type == TokenType.Sentinal)
            {
               if (expectingUnit)
               {
                  // We already have a number, so use the Default unit and add it first, 
                  // before we process this current token.
                  
                  switch (DefaultUnit)
                  {
                  case TimeUnit.Days:
                     ts += TimeSpan.FromDays (amount);
                     break;
                  case TimeUnit.Hours:
                     ts += TimeSpan.FromHours (amount);
                     break;
                  case TimeUnit.Minutes:
                     ts += TimeSpan.FromMinutes (amount);
                     break;
                  case TimeUnit.Seconds:
                     ts += TimeSpan.FromSeconds (amount);
                     break;
                  default:
                     return false;
                  }
               }
               if (token.Type == TokenType.Sentinal)
                  return true;
               Fraction famount;
               if (Fraction.TryParse (token.Value, out famount))
                  amount = famount.ToDouble ();
               else if (!double.TryParse (token.Value, out amount))
                  return false;
               expectingUnit = true;
            }
            else
            {
               return false;
            }
         }
         return true;
      }
      
      private bool TryParseDecimalTimespan (IEnumerable<Token> tokens, out TimeSpan ts)
      {
         ts = new TimeSpan ();
         bool expectingUnit = false;
         
         var enumerator = tokens.GetEnumerator ();
         double amount = 0;
         while (enumerator.MoveNext ())
         {
            Token token = enumerator.Current;
            if (token.Type == TokenType.WhiteSpace)
               continue;
            if (token.Type == TokenType.Character && expectingUnit)
            {
               char first=token.Value [0];
               expectingUnit = false;
               switch (first)
               {
               case 'd':
               case 'D':
                  ts += TimeSpan.FromDays (amount);
                  break;
               case 'h':
               case 'H':
                  ts += TimeSpan.FromHours (amount);
                  break;
               case 'm':
               case 'M':
                  ts += TimeSpan.FromMinutes (amount);
                  break;
               case 's':
               case 'S':
                  ts += TimeSpan.FromSeconds (amount);
                  break;
               default:
                  return false;
               }
            }
            else if (token.Type == TokenType.Number || token.Type == TokenType.Sentinal)
            {
               if (expectingUnit)
               {
                  // We already have a number, so use the Default unit and add it first, 
                  // before we process this current token.
                  
                  switch (DefaultUnit)
                  {
                  case TimeUnit.Days:
                     ts += TimeSpan.FromDays (amount);
                     break;
                  case TimeUnit.Hours:
                     ts += TimeSpan.FromHours (amount);
                     break;
                  case TimeUnit.Minutes:
                     ts += TimeSpan.FromMinutes (amount);
                     break;
                  case TimeUnit.Seconds:
                     ts += TimeSpan.FromSeconds (amount);
                     break;
                  default:
                     return false;
                  }
               }
               if (token.Type == TokenType.Sentinal)
                  return true;
               string normalizedTokenValue = token.Value;
               if (normalizedTokenValue [0] == '.')
                  normalizedTokenValue = "0" + normalizedTokenValue;
               if (normalizedTokenValue [normalizedTokenValue.Length - 1] == '.')
                  normalizedTokenValue = normalizedTokenValue.Substring (0, normalizedTokenValue.Length - 1);
               if (!double.TryParse (normalizedTokenValue, out amount))
                  return false;
               expectingUnit = true;
            }
            else
            {
               return false;
            }
         }
         return true;
      }
      
      private IEnumerable<Token> Tokenize (string s, bool whitespaceIsNumber)
      {
         if (string.IsNullOrEmpty (s))
         {
            yield return new Token {Type=TokenType.Sentinal};
            yield break;
         }
         int pos1 = 0;
         while (pos1 < s.Length)
         {
            char c1 = s[pos1];
            TokenType type1 = GetTokenType (c1, whitespaceIsNumber);
            int pos2;
            for (
               pos2 = pos1 + 1; 
               pos2 < s.Length && type1 == GetTokenType (s[pos2], whitespaceIsNumber);
               pos2++)
            {
            }
            string token = s.Substring (pos1, pos2 - pos1);
            yield return new Token {Value=token,Type=type1};
            pos1 = pos2;
         }
         yield return new Token {Type=TokenType.Sentinal};
      }
      
      private TokenType GetTokenType (char c, bool whitespaceIsNumber)
      {
         TokenType type;
         if (whitespaceIsNumber)
         {
            if (char.IsLetter(c))
               type = TokenType.Character;
            else if (char.IsNumber(c) || c == '/' || char.IsWhiteSpace (c) ||  c == '.')
               type = TokenType.Number;
            else
               type = TokenType.WhiteSpace;
         }
         else
         {
            if (char.IsLetter(c))
               type = TokenType.Character;
            else if (char.IsNumber(c) || c == '.')
               type = TokenType.Number;
            else
               type = TokenType.WhiteSpace;
         }
         return type;
      }
      

      enum TokenType
      {
         Sentinal,
         WhiteSpace,
         Character,
         Number,
      }
      
      struct Token
      {
         public string Value{get;set;}
         public TokenType Type{get;set;}
      }
      #endregion
   }
}
