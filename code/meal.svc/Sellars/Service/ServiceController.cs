using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Service
{
   public class ServiceController
   {
      public static void Put<T> (IService service)
         where T:IService
      {
         Put<T> (typeof(T), service);
      }
      public static void Put<T> (object key, IService service)
         where T:IService
      {
         m_services[key] = service;
      }

      public static T Get<T> ()
         where T:IService
      {
         return Get<T> (typeof(T));
      }
      public static T Get<T> (object key)
         where T:IService
      {
         return (T)m_services[key];
      }

      private static Dictionary<object,IService> m_services = new Dictionary<object,IService> ();
   }
}
