//
// file:    XmlFormatter.cs
// author:  Jeremy Sellars
//
// Proverbs 16:3
// Commit thy works unto the LORD, and thy thoughts shall be established.
//
// XmlFormatter class implementation.
//
// Modifications:
// 25 May 2005  JRS  First checkin.
//

// The XmlFormatter provides xml serialization and deserialization for objects.
//
// Please indicate code changes below.
//
// Author   Date        Description of change
// -------  ----------  ----------------------------------------------------
// JRS      2003-04-17  Removed support for XmlDefaultFieldAttribute per
//                      request by MLM.  Changed enumeration serialization
//                      from element to attribute with numeric value per
//                      request by JTK.
//
// JRS      2003-04-25  Added support for Classes that inherit from other
//                      classes.  Added support for classes defined within
//                      other classes.  The solution for this issue was
//                      using XmlConvert.EncodeName and .DecodeName to
//                      escape invalid characters (such as '+' or '.')
//                      that appear in names of elements or attributes.
//
// JRS      2005-02-07  Modified support for Classes that inherit from other
//                      classes.  When BaseClass defined protected field m_id,
//                      it formerly appeared in the serialization stream for 
//                      SubClass as both m_id and BaseClass_x002B_m_id
//                      simultaneously.  This was changed to only write
//                      BaseClass_x002B_m_id to the stream.
//
// JRS      2005-04-26  Modified support for DateTime, Guid, and primitives.
//                      Formerly, when the attribute value for fields of these
//                      types was empty, a FormatException was thrown.
//                      Now the default value for the type is used instead of
//                      throwing an exception.
//
// JRS      2005-08-23  Modified support for Decimal.  Now serialized in an
//                      attribute instead of an element.
//
// JRS      2006-03-24  Fixed poor support for deserializing arrays of size 0
//                      when serialized like this:
//                      <_list _AType='System.Object' />
//                      Added support for non-public deserialization .ctors.
//
// JRS      2007-01-02  Added support for boolean values of -1 and 0.
//                      Only "true" and "false" strings were supported before.
//                      Improved support for serialization and deserialization
//                      of arrays.  Before, when the root of the object graph
//                      was an array, the serialization stream would not contain
//                      the elements and Deserialize would throw an exception.
//
// JRS      2007-01-17  Added support for floating-point numbers expressed in
//                      scientific notation (Ex: 4.348563345E-4).
//
// JRS      2007-01-14  Added support serialization and deserialization of
//                      generic types.
//


// If defined, PRIMITIVE_AS_ATTRIBUTE indicates that
// simple members (primitives and strings) should
// be represented as attributes of the object xml node
// instead of elements.
// Currently, this disabling this is supported for serialization,
// but not for deserialization.
#define PRIMITIVE_AS_ATTRIBUTE

//#warning consider adding assembly as a possible (optional) output
//#warning consider adding support for n-dimensional arrays

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using NumberStyles=System.Globalization.NumberStyles;

using StringBuilder=System.Text.StringBuilder;

using System.Collections.Generic;

namespace Sellars.Runtime.Serialization
{
#region Class Definition : XmlFormatter

/// <summary>
///   Performs object serialzation and deserialzation using a
///   Softek-standard xml format.
///   This formatter does not support cyclic object graphs.
/// </summary>
public partial class XmlFormatter : Formatter
{
   /// <summary>
   ///   Creates an instance of XmlFormatter.
   /// </summary>
   public
   XmlFormatter ()
   {
      m_typeCache = new Hashtable ();
      m_throwOnCyclic = true;
   }

   /// <summary>
   ///   Deserializes an XML string.
   /// </summary>
   /// <param name="xml">A stream containing xml-encoded objects.</param>
   /// <returns>Deserialized object.</returns>
   public object
   Deserialize (string xml)
   {
      return Deserialize (new StringReader (xml), true);
   }
   
   /// <summary>
   ///   Deserializes an XML string that optionally begins with the 
   ///   XmlDeclaration: &gt;?xml version='1.0'?&lt;
   /// </summary>
   /// <param name="xml">A stream containing xml-encoded objects.</param>
   /// <param name="headerAbsent">Indicates the header is absent.</param>
   /// <returns>Deserialized object.</returns>
   public object
   Deserialize (string xml, bool headerAbsent)
   {
      return Deserialize (new StringReader (xml), headerAbsent);
   }
   
   /// <summary>
   ///   Deserializes an XML stream.
   /// </summary>
   /// <param name="stream">A stream containing xml-encoded objects.</param>
   /// <returns>Deserialized object.</returns>
   public override object
   Deserialize (Stream stream)
   {
      return Deserialize (new StreamReader (stream), true);
   }
   
   /// <summary>
   ///   Deserializes an XML stream while requiring the XmlDeclaration:
   ///   &gt;?xml version='1.0'?&lt;
   /// </summary>
   /// <param name="stream">A stream containing xml-encoded objects.</param>
   /// <param name="headerAbsent">
   ///   Specifies whether the first node should be an xml declaration.
   /// </param>
   /// <returns>Deserialized object.</returns>
   public object
   Deserialize (Stream stream, bool headerAbsent)
   {
      return Deserialize (new StreamReader (stream), headerAbsent);
   }

   /// <summary>
   ///   Deserializes an XML stream that optionally begins with
   ///    the XmlDeclaration: &gt;?xml version='1.0'?&lt;
   /// </summary>
   /// <param name="reader">A stream containing xml-encoded objects.</param>
   /// <param name="headerAbsent">
   ///   Specifies whether the first node should be an xml declaration.
   /// </param>
   /// <returns>Deserialized object.</returns>
   public object
   Deserialize (TextReader reader, bool headerAbsent)
   {
      XmlTextReader  rdr;
      object         ret;
      ArrayList      callbackObjects;
      
      rdr = null;
      try
      {
         rdr = new XmlTextReader (reader);
         rdr.WhitespaceHandling = WhitespaceHandling.None;
         
         // <?xml version='1.0'?>
         // If empty, throw.
         // If this is required and doesn't exist, throw.
         if (!rdr.Read () || 
            (!headerAbsent && rdr.NodeType != XmlNodeType.XmlDeclaration))
         {
            throw CreateException (
               "Header missing from document.", rdr);
         }
         
         // Skip the declaration if it exists.
         if (rdr.NodeType == XmlNodeType.XmlDeclaration && !rdr.Read ())
         {
            throw CreateException (
               "Root node missing from document.", rdr);
         }
      
         // Special logic to support objects with the IDeserializationCallback
         // interface.  If any object in the graph defines a callback, it will
         // be added to this list.
         callbackObjects = new ArrayList (1);

         // Deserialize and return the root object.
         ret = DeserializeOne (rdr, callbackObjects);
         
         // Call the deserialization callback for any object in the list.
         foreach (IDeserializationCallback obj in callbackObjects)
         {
            obj.OnDeserialization (null);
         }
         
         // Now return the object.  Note special handling for Arrays.
         if (ret is XmlFormatter.Support)
            return ((XmlFormatter.Support)ret).Array;
         return ret;
      }
      catch (SerializationException)
      {
         throw;
      }
      catch (XmlException xe)
      {
         throw CreateException ("An error has occurred while parsing.",
                                xe.LineNumber, xe.LinePosition, xe);
      }
      catch (Exception e)
      {
         if (rdr == null)
         {
            throw CreateException ("Unhandled exception.", 0,0, e);
         }
         else
         {
            throw CreateException ("Unhandled exception.", rdr, e);
         }
      }
   }
   
   /// <summary>
   ///   Returns a string containing the serialized object tree.
   /// </summary>
   /// <param name="objectTree">The object tree to serialize.</param>
   /// <returns>A string containing the serialized object tree.</returns>
   public string
   Serialize (object objectTree)
   {
      MemoryStream  stream;
      StreamReader  reader;
      
      stream = null;
      reader = null;

      try
      {
         stream = new MemoryStream ();
         
         Serialize (stream, objectTree);

         reader = new StreamReader (stream);
         
         stream.Position = 0;
         return reader.ReadToEnd ();
      }
      finally
      {
         if (stream != null)
            stream.Close ();
         
         if (reader != null)
            reader.Close ();
      }
   }
   
   /// <summary>
   ///   Serializes the specified object tree to the specified stream.
   /// </summary>
   /// <param name="stream">The ouput stream</param>
   /// <param name="objectTree">The object tree to serialize.</param>
   public override void
   Serialize (Stream stream, object objectTree)
   {
      object  toSerialize;
      if (objectTree == null)
         return;
      
      // Set the helper-function stream and text writer.
      m_doc = new XmlDocument ();
      
      // Since there can be only one root element in an XML document,
      // we must handle root arrays specially.  Use an Array
      if (objectTree.GetType ().IsArray)
         toSerialize = new XmlFormatter.Support (objectTree);
      else
         toSerialize = objectTree;
      
      // Call the other Serialize method with no parent objects.
      m_doc.AppendChild (Serialize (toSerialize, new ArrayList ()));
      
      m_doc.Save (stream);
   }
   
   /// <summary>
   ///   The SerializationBinder used with the current formatter.
   /// </summary>
   public override SerializationBinder
   Binder
   {
      get
      {
         return m_binder;
      }
      
      set
      {
         m_binder = value;
      }
   }
   
   /// <summary>
   ///   The StreamingContext used for the current serialization.
   /// </summary>
   public override ISurrogateSelector
   SurrogateSelector
   {
      get
      {
         return m_selector;
      }
      
      set
      {
         m_selector = value;
      }
   }
   
   /// <summary>
   ///   The ISurrogateSelector used with the current formatter.
   /// </summary>
   public override StreamingContext
   Context
   {
      get
      {
         return m_context;
      }
      
      set
      {
         m_context = value;
      }
   }

   /// <summary>
   ///   Indicates whether serialization should prune the tree when
   ///   it realizes that the graph is cyclic.
   /// </summary>
   public bool
   SupportCyclicGraphs
   {
      get
      {
         return !m_throwOnCyclic;
      }

      set
      {
         m_throwOnCyclic = !value;
      }
   }

   #region Support classes
   /// <summary>
   /// Keeps an array.  This object helps support serialization of root arrays.
   /// </summary>
   [Serializable]
   protected internal class Support
   {
      public
      Support (object array)
      {
         this.Array = array;
      }
      
      public object Array;
   }
   
   protected internal class List : IEnumerable, IList
   {
      public
      List (Type type)
      {
         m_type = type;
         m_list = new ArrayList ();
      }
      
      public
      List (Type type, int capacity)
      {
         m_type = type;
         m_list = new ArrayList (capacity);
      }
      
      public object
      ToList ()
      {
         // GenericIListWrapper<m_type>
         object           wrapper;
         // List<m_type>.ctor (IEnumerable<m_list> list)
         ConstructorInfo  listCtor;
         
         {
         // Get typeof(List<m_type>)
         Type  wrapperType;
         wrapperType = typeof (GenericIListWrapper<>).MakeGenericType (m_type);

         // Find a constructor of GenericIListWrapper<> that takes ArrayList.
         ConstructorInfo  wrapperCtor;
         wrapperCtor = wrapperType.GetConstructor (
            new Type [] {typeof (ArrayList)});
         wrapper = wrapperCtor.Invoke (new object [] {m_list});
         }
         
         {
         // Get typeof(List<m_type>)
         Type  listType;
         listType = 
            typeof (System.Collections.Generic.List<>)
               .MakeGenericType (m_type);

         // Find a constructor of List<> that takes IEnumerable<m_type>.
         Type  ienumerableType;
         ienumerableType = 
            typeof (System.Collections.Generic.IEnumerable<>)
               .MakeGenericType (m_type);
         listCtor = listType.GetConstructor (new Type [] {ienumerableType});
         }
         
         return listCtor.Invoke (new object [] {wrapper});
      }
      
      public IEnumerator
      GetEnumerator ()
      {
         return m_list.GetEnumerator ();
      }
      
      #region IList Members

      public int
      Add (object item)
      {
         return m_list.Add (item);
      }

      public void
      Clear ()
      {
         m_list.Clear ();
      }

      public bool
      Contains (object item)
      {
         return m_list.Contains (item);
      }

      public void
      CopyTo (object [] array, int arrayIndex)
      {
         m_list.CopyTo (array, arrayIndex);
      }

      public int
      Count
      {
         get
         {
            return m_list.Count;
         }
      }

      public bool
      IsReadOnly
      {
         get
         {
            return m_list.IsReadOnly;
         }
      }

      public void
      Remove (object item)
      {
         m_list.Remove (item);
      }

      public int
      IndexOf (object item)
      {
         return m_list.IndexOf (item);
      }

      public void
      Insert (int index, object item)
      {
         m_list.Insert (index, item);
      }

      public void
      RemoveAt (int index)
      {
         m_list.RemoveAt (index);
      }

      public object
      this [int index]
      {
         get
         {
            return m_list [index];
         }
         set
         {
            m_list [index] = value;
         }
      }
      public bool IsSynchronized
      {
         get
         {
            return false;
         }
      }

      public object SyncRoot
      {
         get
         {
            return this;
         }
      }

      public bool
      IsFixedSize
      {
         get
         {
            return m_list.IsFixedSize;
         }
      }

      public void
      CopyTo (Array array, int index)
      {
         m_list.CopyTo (array, index);
      }

      #endregion

      private class GenericIListWrapper<T>
         : System.Collections.Generic.IList<T>
      {
         public
         GenericIListWrapper (ArrayList list)
         {
            m_list = list;
         }
         
         public int
         Count
         {
            get
            {
               return m_list.Count;
            }
         }

         public bool
         IsReadOnly
         {
            get
            {
               return m_list.IsReadOnly;
            }
         }

         public T
         this [int index]
         {
            get
            {
               return (T) m_list [index];
            }
            set
            {
               m_list [index] = value;
            }
         }

         public void
         Add (T item)
         {
            m_list.Add (item);
         }

         public void
         Clear ()
         {
            m_list.Clear ();
         }

         public bool
         Contains (T item)
         {
            return m_list.Contains (item);
         }

         public void
         CopyTo (T [] array, int arrayIndex)
         {
            m_list.CopyTo (array, arrayIndex);
         }

         public bool
         Remove (T item)
         {
            int index = m_list.IndexOf (item);
            if (index == -1)
               return false;

            m_list.RemoveAt (index);
            return true;
         }

         public int
         IndexOf (T item)
         {
            return m_list.IndexOf (item);
         }

         public void
         Insert (int index, T item)
         {
            m_list.Insert (index, item);
         }

         public void
         RemoveAt (int index)
         {
            m_list.RemoveAt (index);
         }

         public System.Collections.Generic.IEnumerator<T>
         GetEnumerator ()
         {
            return new Enumerator (m_list);
         }

         IEnumerator
         IEnumerable.GetEnumerator ()
         {
            return this.GetEnumerator ();
         }
         
         private class Enumerator : System.Collections.Generic.IEnumerator<T>
         {
            public
            Enumerator (ArrayList list)
            {
               m_enumerator = list.GetEnumerator ();
            }
            
            public T Current
            {
               get
               {
                  return (T) m_enumerator.Current;
               }
            }

            public void
            Dispose ()
            {
               // Nothing to cleanup.
            }

            object
            IEnumerator.Current
            {
               get
               {
                  return m_enumerator.Current;
               }
            }

            public bool
            MoveNext ()
            {
               return m_enumerator.MoveNext ();
            }

            public void
            Reset ()
            {
               m_enumerator.Reset ();
            }

            private IEnumerator  m_enumerator;
         }
         
         private ArrayList  m_list;
      }
      
      private Type       m_type;
      private ArrayList  m_list;
   }

   #endregion Support classes
   
   #region Unimplemented Protected members inherited from Formatter.
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="obj"></param>
   /// <param name="name"></param>
   /// <param name="memberType"></param>
   protected override void
   WriteArray (object obj, string name, Type memberType)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteBoolean (bool val, string name)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteByte (Byte val, string name)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteChar (char val, string name)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteDateTime (DateTime val, string name)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteDecimal (Decimal val, string name)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteDouble (Double val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteInt16 (short val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteInt32 (int val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteInt64 (long val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteSByte (SByte val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteSingle (Single val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteTimeSpan (TimeSpan val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteUInt16 (ushort val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteUInt32 (uint val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   protected override void
   WriteUInt64 (ulong val, string name)
   {}
   
   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   /// <param name="memberType"></param>
   protected override void
   WriteObjectRef (object val, string name, Type memberType)
   {}

   /// <summary>
   ///   Not implemented.
   /// </summary>
   /// <param name="val"></param>
   /// <param name="name"></param>
   /// <param name="memberType"></param>
   protected override void
   WriteValueType (object val, string name, Type memberType)
   {}
   #endregion

   /// <summary>
   ///   Deserializes an XML stream without requiring the XmlDeclaration:
   ///   &gt;?xml version='1.0'?&lt;
   /// </summary>
   /// <param name="rdr">A XmlTextReader at the current position.</param>
   /// <param name="callbackObjects">
   ///   A list of objects for which OnDeserialization should be executed when
   ///   the object graph is deserialized.
   /// </param>
   /// <returns>Deserialized object.</returns>
   protected object
   DeserializeOne (XmlTextReader rdr, ArrayList callbackObjects)
   {
      // DeserializeOne (XmlTextReader, out string) does the following.
      // 1.  Gets a Type for the class name and namespace from the current node.
      // 2.  Builds a Hashtable containing member-value pairs. (recursive)
      // 3.  Creates and populates the object using the fields hashtable.
      
      string     classname;
      Type       objType;
      Hashtable  xmlfields;
      bool       container;
      object     ret;

      // Determine the class name from the name of the current node.
      classname = XmlConvert.DecodeName (rdr.Name);
      // Determine if the element is not empty. i.e. <SomeNode SomeAttr="" />
      container = !rdr.IsEmptyElement;
      
      if (rdr.NodeType != XmlNodeType.Element)
      {
         // Only deserialize from Element nodes.
         throw CreateException (string.Format (
            "Cannot retrieve object data from node. type: {0}.",
            rdr.NodeType.ToString ()), rdr);
      }
      else if (classname == ELEM_NULL && rdr.AttributeCount == 0)
      {
         // Serialized null value.
         return null;
      }
      else if (rdr.HasAttributes && rdr.MoveToAttribute (ATTR_NS))
      {
         // Get the namespace of the object and the Type of the object.

         // This will result in null if the type is not found
         // for the specified name and namespace.
         // Get the namespace and generic arguments.
         string  ns = rdr.Value;
         string  ga = rdr.MoveToAttribute (ATTR_GA) ? rdr.Value : null;
         objType = GetObjectType (classname, ns, ga);
      }
      else if (IsSimple (classname))
      {
         ret = DeserializeSimple (rdr, false, callbackObjects);

         // Special logic to support objects with the IDeserializationCallback
         // interface.  If the object defines a callback, add it to the list.
         if (ret is IDeserializationCallback)
         {
            callbackObjects.Add (ret);
         }

         // Now return the object.
         return ret;
      }
      else
      {
         // CreateException because the node does not contain a namespace.
         throw CreateException (
            "Expected namespace in node: " + classname + ".", rdr);
      }
      
      if (objType == null)
      {
         throw CreateException (string.Format (
            "Class not found: '{0}' in namespace '{1}'.",
            classname, rdr.Value), rdr);
      }
      else if (!objType.IsSerializable)
      {
         // If the object is not serializable, throw a meaningful exception.
         throw CreateException (string.Format (
            "The type {0} in {1} is not marked as serializable.",
            objType.FullName, objType.Assembly.FullName), rdr);
      }
      else
      {
         // Append attributes to xmlfields.
         bool  proceed;
         xmlfields = new Hashtable (rdr.AttributeCount);
         proceed = rdr.MoveToFirstAttribute ();
         while (proceed)
         {
            string  name = XmlConvert.DecodeName (rdr.Name);
            if (name != ATTR_NS &&
                name != ATTR_GA &&
                name != ATTR_ARRAY_TYPE &&
                name != ATTR_ARRAY_CAPACITY)
            {
               xmlfields [name] = rdr.Value;
            }
            proceed = rdr.MoveToNextAttribute ();
         }
      }

      // The node has inner markup or text.
      // Indirect recursion.
      if (container)
      {
         // Populates xmlfields with the values contained in sub-elements.

         // Advance the xml reader through the contents, but discard them.
         DeserializeMany (rdr, xmlfields, callbackObjects);
      }
      
      // Return constructed object.
      
      ret = ConstructObject (objType, xmlfields);

      // Special logic to support objects with the IDeserializationCallback
      // interface.  If the object defines a callback, add it to the list.
      if (ret is IDeserializationCallback)
      {
         callbackObjects.Add (ret);
      }

      // Now return the object.
      return ret;
   }
   
   /// <summary>
   ///   Parses object nodes until the parent's end element is encountered.
   /// </summary>
   /// <param name="rdr">
   ///   Reader positioned at the parent of the firstobject to serialize.
   /// </param>
   /// <param name="xmlfields">
   ///   A hash table containing field-value pairs.
   ///   Set to null if fields are not expected.
   /// </param>
   /// <param name="callbackObjects">
   ///   A list of objects for which OnDeserialization should be executed when
   ///   the object graph is deserialized.
   /// </param>
   /// <returns></returns>
   protected object
   DeserializeMany (XmlTextReader rdr, Hashtable xmlfields,
                    ArrayList callbackObjects)
   {
      // 0.  The reader is positioned at the element containing the list.
      //     If xmlfields is not null, look for fields among contents list.
      // 1.  Enumerate child xml nodes.
      //     a.  If the node is Text or CData, add string to contents.
      //     b.  If the node is an Element
      //         i.  If represents object, add deserialized object to contents.
      //         ii. Else If is field deserialize list to fields
      //         iii.Else deserialize simple object as contents.
      // 2.  Return null, object, TypedList, List<>, or array as appropriate.
      
      IList   contents;
      string  contentTypeStr;
      Type    contentType;
      bool    proceed;
      string  capacity;
      
      // TODO: Support empty object tags.
      
      // Get the type of the contents array or empty/null if not an array.
      contentTypeStr = rdr.GetAttribute (ATTR_ARRAY_TYPE);
      capacity = rdr.GetAttribute (ATTR_ARRAY_CAPACITY);
      rdr.MoveToElement ();
      if (XmlConvert.DecodeName (rdr.Name) == ELEM_NULL)
         return null;

      if (rdr.IsEmptyElement)
      {
         proceed = false;
      }
      else
      {
         proceed = rdr.Read ();
      }

      // Initialize contents to an appropriate capacity.
      if (contentTypeStr == null)
      {
         contents = new ArrayList (1);
      }
      else
      {
         // TypedLists cannot contain null and therefore are not appropriate
         // for general arrays.

         // If the array is a TypedList, initialize contents to a TypedList.
         if (contentTypeStr.StartsWith (PREFIX_LIST))
         {
            contentTypeStr = contentTypeStr.Substring (PREFIX_LIST.Length);
            contentType = GetObjectType (contentTypeStr);

            if (string.IsNullOrEmpty (capacity))
               contents = new List (contentType);
            else
               contents = new List (contentType, int.Parse (capacity));
         }
         else if (contentTypeStr.StartsWith (PREFIX_TYPEDLIST))
         {
            contentTypeStr = contentTypeStr.Substring (PREFIX_TYPEDLIST.Length);
            contentType = GetObjectType (contentTypeStr);

            if (string.IsNullOrEmpty (capacity))
               contents = new TypedList (contentType);
            else
               contents = new TypedList (contentType, int.Parse (capacity));
         }
         else
         {
            // Else, initialize contents to an ArrayList.
            contentType = GetObjectType (contentTypeStr);

            if (string.IsNullOrEmpty (capacity))
               contents = new ArrayList ();
            else
               contents = new ArrayList (int.Parse (capacity));
         }
      }
      
      // Loop through children.
      rdr.MoveToContent ();
      while (proceed && rdr.NodeType != XmlNodeType.EndElement)
      {
         switch (rdr.NodeType)
         {
         case XmlNodeType.Element:
            // Not explicitly an object, so process the field as a list.
            if (rdr.GetAttribute (ATTR_NS) == null)
            {
               string  name = XmlConvert.DecodeName (rdr.Name);
               // Are we expecting a field?
               if (xmlfields == null || IsSimple (name))
               {
                  contents.Add (DeserializeSimple (rdr, false,
                                                   callbackObjects));
               }
               else
               {
                  xmlfields [name] = DeserializeSimple (rdr, true,
                                                        callbackObjects);
               }
            }
            else
            {
               object val;
               // Indirect Recursion.
               val = DeserializeOne (rdr, callbackObjects);
               if (val != null)
                  contents.Add (val);
            }
            break;
            
         case XmlNodeType.CDATA:
         case XmlNodeType.Text:
            contents.Add (rdr.ReadString ());
            break;
         } // end switch.
         
         // Advance to the next XmlNode.
         proceed = rdr.Read ();
         rdr.MoveToContent ();
      } // end while.
      
      // Return the object or null.
      if (string.IsNullOrEmpty (contentTypeStr))
      {
         if (contents.Count == 0)
            return null;
         else
            return contents [0];
      }
      else
      {
         // Return a TypedList or array of the specified type.
         // Determine if Array or TypedList.
         if (contents is List)
         {
            // Convert from the list wrapper to 
            // System.Collections.Generic.List<correctType>.
            return ((List)contents).ToList ();
         }
         else if (contents is TypedList)
         {
            return contents;
         }
         else
         {
            contentType = GetObjectType (contentTypeStr);
            if (contents is ArrayList)
            {
               if (contentType == null)
                  return ((ArrayList) contents).ToArray ();
               else
                  return ((ArrayList) contents).ToArray (contentType);
            }
            else
            {
               if (contentType == null)
                  return ((TypedList) contents).ToArray ();
               else
                  return ((TypedList) contents).ToArray (contentType);
            }
         }
      }
   }
   
   /// <summary>
   ///   Deserializes the object on which the reader is positioned.
   ///   Returns null if the object is neither primitive nor a string.
   /// </summary>
   /// <param name="rdr">
   ///   An XmlTextReader positioned on the node to deserialize.
   /// </param>
   /// <param name="elseMany">
   ///   Indicates whether to deserialize many if not simple.
   /// </param>
   /// <param name="callbackObjects">
   ///   A list of objects for which OnDeserialization should be executed when
   ///   the object graph is deserialized.
   /// </param>
   /// <returns>
   ///   The deserialized object or null if the node is not simple.
   /// </returns>
   protected object
   DeserializeSimple (XmlTextReader rdr, bool elseMany,
                      ArrayList callbackObjects)
   {
      // 0.  The reader is positioned at the element containing the list.
      // 1.  Get the type of the primitive or string from the node name.
      // 2.  If the type is primitive, parse and return.  Else, throw.

      string name;
      
      // 1.  Get the type of the primitive or string from the node name.
      rdr.MoveToElement ();
      name = XmlConvert.DecodeName (rdr.Name);

      // 2.  If the type is primitive, parse and return.  Else, throw.
      if (name == ELEM_NULL)
         return null;
      else if (name == "String")
         return rdr.ReadString ();
      else if (name == "Boolean")
         return DeserializeBooleanString (rdr.ReadString ());
      else if (name == "DateTime")
         return DateTime.Parse (rdr.ReadString ());
      else if (name == "Decimal")
         return ParseDecimal (rdr.ReadString ());
      else if (name == "Guid")
         return new Guid (rdr.ReadString ());
      else if (name == "Byte")
         return Byte.Parse (rdr.ReadString ());
      else if (name == "Int16")
         return Int16.Parse (rdr.ReadString ());
      else if (name == "Int32")
         return Int32.Parse (rdr.ReadString ());
      else if (name == "Int64")
         return Int64.Parse (rdr.ReadString ());
      else if (name == "SByte")
         return SByte.Parse (rdr.ReadString ());
      else if (name == "UInt16")
         return UInt16.Parse (rdr.ReadString ());
      else if (name == "UInt32")
         return UInt32.Parse (rdr.ReadString ());
      else if (name == "UInt64")
         return UInt64.Parse (rdr.ReadString ());
      else if (name == "Char")
         return Char.Parse (rdr.ReadString ());
      else if (name == "Double")
         return ParseDouble (rdr.ReadString ());
      else if (name == "Single")
         return ParseSingle (rdr.ReadString ());
      else if (elseMany)
         return DeserializeMany (rdr, null, callbackObjects);
      else
         throw CreateException ("Invalid simple object.", rdr);
   }

   /// <summary>
   ///   Determines whether the specified type name is primitive or a string.
   /// </summary>
   /// <param name="name">
   ///   Name of the node.
   /// </param>
   /// <returns>
   ///   A value indicating whether the name represents a simple type.
   /// </returns>
   protected bool
   IsSimple (string name)
   {
      return (
         name == ELEM_NULL
      || name == "String"
      || name == "Boolean"
      || name == "DateTime"
      || name == "Decimal"
      || name == "Guid"
      || name == "Byte"
      || name == "Int16"
      || name == "Int32"
      || name == "Int64"
      || name == "SByte"
      || name == "UInt16"
      || name == "UInt32"
      || name == "UInt64"
      || name == "Char"
      || name == "Double"
      || name == "Single");
   }

   #region Protected Serialize methods and helpers
   /// <summary>
   ///   Serializes the object to the module-level stream
   ///   unless the specified object exists inside the parents list.
   /// </summary>
   /// <param name="objTree">The object to serialize</param>
   /// <param name="parents">
   ///   The objects being serialized inside the call stack.
   /// </param>
   protected XmlElement
   Serialize (object objTree, IList parents)
   {
      // Serialize (object, IList) has these steps:
      // 1.  If objTree is null, return <NullObject/>.
      // 2.  If objTree is contained in parents, perform cyclic graph behavior.
      // 3.  If objTree is not serializable, throw.
      // 4.  Initialize an XmlNode to contain the serialization of this object.
      // 5.  If objTree is simple:
      //     a.  If is primitave, write ToString to the InnerText.
      //     b.  If is string, write ToString to the InnerText as CData.
      // 6.  Enumerate the serializable members
      //     a.  If the member is simple, put it in an attribute.
      //     b.  Else If the member is an Array then
      //         i.  If objTree Is IList, do nothing.
      //         ii. Else If member Is string, write xml-formatted to InnerXml.
      //         iii.Else "<FieldName>" + Serialized objects + "</FieldName>"
      //     c.  Else objTree is composite, Add to contents:
      //         '<FieldName _NS="{ns}">' + Serialized Member + '</FieldName>'
      // 7.  return XmlNode.

      // Rule 1.
      if (parents == null)
         throw new ArgumentNullException ();
      if (objTree == null)
         return CreateNullElement ();
         
      // Rule 2. Handle cyclic object graphs.
      // There are several ways of handling a cyclic object graph using this
      // format.
      //    a.  Continue recursing until a StackOverflow occurs.
      //    b.  Throw now instead of later to prevent the processor wastage.
      //    c.  Return null instead of throwing.
      //    d.  Return objTree.ToString () instead of throwing.
      // The options that provide the most expected behavior are (b) and (c).
      // The m_throwOnCyclic flag provides these options.
      if (parents.Contains (objTree))
      {
         if (m_throwOnCyclic)
         {
            // Use option (b).  Simply throw.
            throw new NotSupportedException ("Serialize cyclic object graph.");
         }
         else
         {
            // Use option (c).  Return a null element.
            return CreateNullElement ();
         }
      }
      parents.Add (objTree);
      
      // Don't catch, but remove objTree from parents in Finally clause.
      try
      {
         // The local, type, refers to the Type of objTree.
         Type    type;
         string  typename;

         // node contains the serialization of objTree.
         XmlElement    node;
         XmlAttribute  attrNS; // Namespace "_NS"
         XmlAttribute  attrGA; // Generic (type) arguments "_GA"
         string        @namespace;
         string        genericArgs;
         
         type = objTree.GetType ();
         typename = GetTypeName (type, out @namespace, out genericArgs);
         
         // Step 3.
         // If the object is not serializable, throw.
         if (!type.IsSerializable)
         {
            // CreateException meaningful exception.
            throw new SerializationException (
               "The type " + type.FullName + " in " + 
               type.Assembly.FullName +
               " is not marked as serializable.");
         }

         // Step 4.
         node = m_doc.CreateElement (XmlConvert.EncodeName (typename));

         // Step 5.a.
         if (type.IsPrimitive)
         {
            node.AppendChild (m_doc.CreateTextNode (objTree.ToString ()));
         }
         else if (type == typeof (DateTime))
         {
            node.AppendChild (m_doc.CreateTextNode (
               FormatDate ((DateTime) objTree)));
         }
         else if (type == typeof (Decimal))
         {
            node.AppendChild (m_doc.CreateTextNode (objTree.ToString ()));
         }
         else if (type == typeof (Guid))
         {
            node.AppendChild (m_doc.CreateTextNode (objTree.ToString ()));
         }
         else if (type == typeof (string))
         {
            // Step 5.b.
            node.AppendChild (m_doc.CreateCDataSection (objTree.ToString ()));
         }
         else
         {
            // Step 6.  Enumerate the serializable members
            SerializationInfo        info;
            ISerializationSurrogate  surrogate = null;

            attrNS = m_doc.CreateAttribute (ATTR_NS);
            attrNS.Value = type.Namespace;
            node.Attributes.Append (attrNS);

            if (genericArgs != null)
            {
               attrGA = m_doc.CreateAttribute (ATTR_GA);
               attrGA.Value = genericArgs;
               node.Attributes.Append (attrGA);
            }
            
            // Get a serialization surrogate if one has been registered.
            if (m_selector != null)
            {
               ISurrogateSelector  selector;
               surrogate = m_selector.GetSurrogate (
                  type, m_context, out selector);
            }

            // If available, use a surrogate to get the serialization info.
            if (surrogate != null)
            {
               // Use the surrogate to get the serialization information
               info = new SerializationInfo (objTree.GetType (),
                  XmlFormatterConverter.Instance);
               surrogate.GetObjectData (objTree, info, m_context);
            }
            else if (objTree is ISerializable)
            {
               // If the object is ISerializable, serialize the object data.
               ISerializable  obj;
               
               obj = (ISerializable) objTree;
               info = new SerializationInfo (type,
                                             XmlFormatterConverter.Instance);
               obj.GetObjectData (info, m_context);
            }
            else
            {
               // If the object is not ISerializable, use FormatterServices.
               System.Reflection.MemberInfo []  members;
               object []                        data;
               
               info = new SerializationInfo (type,
                                             XmlFormatterConverter.Instance);

               // GetSerializableMembers throws a SerializationException
               // if type is not serializable.
               members = FormatterServices.GetSerializableMembers (type);
               data = FormatterServices.GetObjectData (objTree, members);
               for (int i=0; i < data.Length; i++)
               {
                  // JRS      2005-02-07
                  // Modified support for Classes that inherit from other
                  // classes.  When BaseClass defined protected field m_id,
                  // it formerly appeared in the serialization stream for 
                  // SubClass as both m_id and BaseClass_x002B_m_id
                  // simultaneously.  This was changed to only write
                  // BaseClass_x002B_m_id to the stream.
                  // The declaring type and reflected type will not match
                  // when looking at a protected or public member
                  // of the base class.  We use this fact to skip
                  // protected and public fields exposed by the base class.
                  if (members [i].DeclaringType == members [i].ReflectedType)
                  {
                     info.AddValue (members [i].Name, data [i]);
                  }
               }
            }
            
            // Writes the member information as attributes and child elements 
            // of node.
            WriteMembers (node, info, parents);
         }
         
         // Step 7.
         return node;
      }
      // Don't catch anything, but remove objTree from parents.
      finally
      {
         parents.Remove (objTree);
      }
   }

   protected static string
   GetTypeName (Type type, out string @namespace, out string genericArgs)
   {
      StringBuilder  nameBuilder;  // Only create for nested types.
      Type           parent;
      
      @namespace = type.Namespace;

      if (type.IsGenericType)
         genericArgs = JoinTypeNames (type.GetGenericArguments (), ",");
      else
         genericArgs = null;

      parent = type.DeclaringType;
      // Short-curcuit here unless it's a nested type.
      if (parent == null)
         return type.Name;
         
      // Enumerate declaring types (in which this type is nested)
      // So, a type expressed in C# as Grandparent.Parent.Child
      // will be represented "Grandparent+Parent+Child".
      nameBuilder = new StringBuilder ();
      for (; parent != null; parent = parent.DeclaringType)
      {
         nameBuilder.Insert (0, parent.Name + "+");
      }
      nameBuilder.Append (type.Name);
      
      return nameBuilder.ToString ();
   }
   
   protected static string
   JoinTypeNames (Type [] types, string delimeter)
   {
      StringBuilder  name;

      if (types == null || types.Length == 0)
         return null;
      
      // 0. Add the first type.
      name = new StringBuilder ();
      name.Append (GetFullyQualifiedTypeName (types [0]));
      for (int i = 1; i < types.Length; i++)
      {
         // 1-n. Add a delemiter, then the type name.
         name.Append (delimeter);
         name.Append (GetFullyQualifiedTypeName (types [i]));
      }
      
      return name.ToString ();
   }
   
   protected internal static string
   GetFullyQualifiedTypeName (Type type)
   {
      // The goal is to write a string like this:
      // Non-generic Example:
      //   "Windows.Forms.ListView"
      // Generic Example:
      //   "Windows.Forms.ListView`1[System.DateTime]"
      // Non-generic Member Type Example:
      //   "Windows.Forms.ListView+Item"
      // Non-generic Member Type of Generic Type Example:
      //   "Windows.Forms.ListView`1[System.DateTime]+Item"
      // Generic Member Type of Generic Type Example:
      //   "Windows.Forms.ListView`1[System.DateTime]+Item[System.String]"

      StringBuilder  name = new StringBuilder ();
      
      // Start of recursion.
      GetFullyQualifiedTypeName (type, type, name);
      
      return name.ToString ();
   }

   /// <summary>
   /// Gets a string name of a type that can be used in _ATyp and _GA attributes.
   /// </summary>
   /// <param name="type">The type to name.</param>
   /// <param name="name">Stringbuilder in which to populate the name.</param>
   protected static void
   GetFullyQualifiedTypeName (Type rootType, Type type, StringBuilder name)
   {
      if (type == null)
         throw new ArgumentNullException ("type");
      if (name == null)
         throw new ArgumentNullException ("name");
      
      // First, write any declaring type information.
      if (type.IsArray)
      {
         // Assumption: If we are in this branch,
         // this is the first time we've been called
         // by GetFullyQualifiedTypeName (Type type).
#if DEBUG
         if (rootType != type)
            type.ToString ();
#endif

         rootType = type.GetElementType ();
         GetFullyQualifiedTypeName (rootType, rootType, name);
         name.Append ("[]");
         return;
      }
      else if (type.DeclaringType == null)
      {
         if (type.Namespace != null)
         {
            name.Append (type.Namespace);
            name.Append (DELIMITER_NAMESPACE);
         }
      }
      else
      {
         GetFullyQualifiedTypeName (rootType, type.DeclaringType, name);
         name.Append (DELIMITER_FULLY_QUALIFIED_TYPE_NAME);
      }
      
      // Now, append the "typeName[genericArgs]".
      name.Append (type.Name);
      
      // Only write the generic arguments for the type we are reflecting.
      if (rootType == type)
      {
         Type []  genericArguments = type.GetGenericArguments ();
         
         if (genericArguments != null && genericArguments.Length != 0)
         {
            // These are already appended.
            //// name.Append ('`');
            //// name.Append (genericArguments.Length);
            name.Append ('[');
            name.Append (JoinTypeNames (genericArguments, ","));
            name.Append (']');
         }
      }
   }
   
   /// <summary>
   ///   Writes each member to the specified node.
   /// </summary>
   /// <param name="node">Node containing members.</param>
   /// <param name="info">Member information.</param>
   /// <param name="parents">Stack of objects being serialized.</param>
   protected void
   WriteMembers (XmlElement node, SerializationInfo info, IList parents)
   {
      foreach (SerializationEntry m in info)
      {
         WriteMember (node, parents, m.Name, m.Value, m.ObjectType);
      }   
   }
   
   /// <summary>
   ///   Writes each member to the specified node.
   /// </summary>
   /// <param name="node">Node containing members.</param>
   /// <param name="parents">Stack of objects being serialized.</param>
   /// <param name="name">Member name.</param>
   /// <param name="val">Member value.</param>
   /// <param name="type">Member type.</param>
   protected void
   WriteMember (XmlElement node, IList parents, 
                string name, object val, Type type)
   {
      //   a.  If the member is simple, put it in an attribute.
      //   b.  Else If the member is an Array then WriteMember (val._items).
      //   c.  Else If the member is an Array then
      //      i.  If objectTree Is IList, do nothing.
      //      ii. Else If member Is string, write xml-formatted to InnerXml.
      //      iii.Else "<FieldName>" + Serialized objects + "</FieldName>"
      //   d.  Else objectTree is composite, Add to contents:
      //            '<FieldName NetNs="{namespace}">' + Serialized Member
      //            + '</FieldName>'

#if (PRIMITIVE_AS_ATTRIBUTE)
      if (type.IsPrimitive || val is String)
      {
         XmlAttribute  attr;
         
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (name));
         attr.Value = val.ToString ();
         node.Attributes.Append (attr);
      }
      else if (val is DateTime)
      {
         XmlAttribute  attr;
         
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (name));
         attr.Value = FormatDate ((DateTime) val);
         node.Attributes.Append (attr);
      }
      else if (val is Decimal)
      {
         XmlAttribute  attr;
         
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (name));
         attr.Value = val.ToString ();
         node.Attributes.Append (attr);
      }
      else if (val is Guid)
      {
         XmlAttribute  attr;
         
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (name));
         attr.Value = val.ToString ();
         node.Attributes.Append (attr);
      }
      else if (type.IsEnum)
      {
         // Write the numeric value of the enumeration as an attribute.

         XmlAttribute  attr;
         string        enumval;
         
         // In the unlikely event that the basetype of the enum
         // would not fit in a long, convert it to an unsigned long.
         // Every other basetype would fit within a signed long.
         if (val is ulong)
            enumval = Convert.ToUInt64 (val).ToString ();
         else
            enumval = Convert.ToInt64 (val).ToString ();
         
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (name));
         attr.Value = enumval;
         node.Attributes.Append (attr);
      }
      else
#endif
      {
         XmlElement  element;
         
         // Initialize <FieldName> element.
         element = m_doc.CreateElement (XmlConvert.EncodeName (name));
         if (type.IsArray)
         {
            XmlAttribute  attr;
            attr = m_doc.CreateAttribute (XmlConvert.EncodeName (ATTR_ARRAY_TYPE));
            attr.Value = GetFullyQualifiedTypeName (type.GetElementType ());
            element.Attributes.Append (attr);
         }

         // do something with element
         if (val == null)
            element.AppendChild (CreateNullElement ());
         else
            WriteObject (element, parents, val, type, false);

         // Append the node to the parent.
         node.AppendChild (element);
      }
   }
   
   /// <summary>
   ///   Writes each member to the specified node.
   /// </summary>
   /// <param name="element">Node containing members.</param>
   /// <param name="parents">Stack of objects being serialized.</param>
   /// <param name="obj">Member value.</param>
   /// <param name="type">Member type.</param>
   /// <param name="wrapPrimitive">
   ///   Indicates whether to create a create a node
   ///   to contain primitive fields.
   /// </param>
   protected void
   WriteObject (XmlElement element, IList parents, 
                object obj, Type type, bool wrapPrimitive)
   {
      // If primitive, '<name>' + ToString () + '</name>'.
      if (type.IsPrimitive)
      {
         if (wrapPrimitive)
         {
            XmlElement  node;
            
            node = m_doc.CreateElement (type.Name);
            
            // If the compiler directive suggests that the xml
            // should be element-centric.
            #if (!PRIMITIVE_AS_ATTRIBUTE)
            {
               XmlAttribute attrNS;

               attrNS = m_doc.CreateAttribute (ATTR_NS);
               attrNS.Value = type.Namespace;
               node.Attributes.Append (attrNS);
            }
            #endif
            node.AppendChild (m_doc.CreateTextNode (obj.ToString ()));
            element.AppendChild (node);
         }
         else
         {
            element.AppendChild (m_doc.CreateTextNode (obj.ToString ()));
         }
      }
      else if (type == typeof (string))
      {
         // If string, '<name>' + CData(ToString ()) + '</name>'.
         element.AppendChild (
            m_doc.CreateCDataSection (obj.ToString ()));
      }
      else if (type == typeof (DateTime))
      {
         // If Date, '<name>' + ToString () + '</name>'.
         XmlElement  dtnode;
         
         dtnode = m_doc.CreateElement ("DateTime");
         dtnode.AppendChild (
            m_doc.CreateTextNode (FormatDate ((DateTime) obj)));
         element.AppendChild (dtnode);
      }
      else if (type == typeof (Decimal))
      {
         // If Decimal, '<name>' + ToString () + '</name>'.
         XmlElement  decimalnode;
         
         decimalnode = m_doc.CreateElement ("Decimal");
         decimalnode.AppendChild (
            m_doc.CreateTextNode (obj.ToString ()));
         element.AppendChild (decimalnode);
      }
      else if (type == typeof (Guid))
      {
         // If Guid, '<name>' + ToString () + '</name>'.
         XmlElement  guidnode;
         
         guidnode = m_doc.CreateElement ("Guid");
         guidnode.AppendChild (
            m_doc.CreateTextNode (obj.ToString ()));
         element.AppendChild (guidnode);
      }
      else if (type.IsArray)
      {
         // If array, '<name>' + Serialize (item [0]) + ... + '</name>'.
         XmlAttribute  attr;
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (ATTR_ARRAY_TYPE));
         attr.Value = GetFullyQualifiedTypeName (type.GetElementType ());
         element.Attributes.Append (attr);

         foreach (object o in (Array) obj)
         {
            element.AppendChild (o == null
               ? CreateNullElement ()
               : Serialize (o, parents));
         }
      }
      else if (type == typeof (TypedList))
      {
         // If typedlist, '<name>' + Serialize (item [0]) + ... + '</name>'.
         XmlAttribute  attr;
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (ATTR_ARRAY_TYPE));
         attr.Value = PREFIX_TYPEDLIST + ((TypedList) obj).EnclosedType.FullName;
         element.Attributes.Append (attr);

         foreach (object o in (TypedList) obj)
         {
            // o will never be null.
            element.AppendChild (Serialize (o, parents));
         }
      }
      else if (type.IsGenericType && 
         type.GetGenericTypeDefinition ()
            == typeof (System.Collections.Generic.List<>))
      {
         // If List<>, '<name>' + Serialize (item [0]) + ... + '</name>'.
         XmlAttribute  attr;
         attr = m_doc.CreateAttribute (XmlConvert.EncodeName (ATTR_ARRAY_TYPE));
         attr.Value = PREFIX_LIST + 
            GetFullyQualifiedTypeName (type.GetGenericArguments () [0]);
         element.Attributes.Append (attr);

         foreach (object o in (IEnumerable) obj)
         {
            // o will never be null.
            element.AppendChild (Serialize (o, parents));
         }
      }
      else
      {
         // If wrapping object in <FieldName></FieldName>.
         element.AppendChild (Serialize (obj, parents));
      }
   }
   
   /// <summary>
   ///   Creates an empty element with the name specified in const ELEM_NULL.
   /// </summary>
   /// <returns></returns>
   protected XmlElement
   CreateNullElement ()
   {
      return m_doc.CreateElement (ELEM_NULL);
   }
   
   /// <summary>
   ///   Formats a DateTime object as a human-readable string.
   /// </summary>
   /// <param name="dt">The DateTime to format.</param>
   /// <returns>A human-readable date/time string.</returns>
   protected string
   FormatDate (DateTime dt)
   {
      // Visual Basic (pre-.Net) is able to parse dates in this format:
      //    dt.ToShortDateString () + " " + dt.ToLongTimeString ();
      // In March 2003, it was deemed necessary to include fractional seconds
      // in the format for datetimes.  The format was established to be
      //    4-digit year, dash, 2-digit month, dash, 2-digit date, dash, space,
      //    2-digit 24-hour hour, colon, 2-digit minutes, colon,
      //    2-digit seconds, decimal (.), 4-digit fractional seconds.
      // This format can be expressed by the format string:
      //    "yyyy-MM-dd HH:mm:ss.ffff".
      // and can be parsed by the DateTime.Parse () and should be
      // culture-independant.
      
      return dt.ToString ("yyyy-MM-dd HH:mm:ss.ffff");
   }
   #endregion Protected Serialize methods and helpers

   /// <summary>
   /// A class that uses extends the standard FormatterConverter
   /// for custom xml deserialization that the XmlFormatter supports.
   /// </summary>
   protected internal class XmlFormatterConverter
      : FormatterConverter, IFormatterConverter
   {
      public static readonly XmlFormatterConverter  Instance =
         new XmlFormatterConverter ();
      
      object
      IFormatterConverter.Convert (object value, TypeCode typeCode)
      {
         if (typeCode == TypeCode.Boolean)
            return ((IFormatterConverter)this).ToBoolean (value);
         return base.Convert (value, typeCode);
      }

      object
      IFormatterConverter.Convert (object value, Type type)
      {
         if (type == typeof (Boolean))
            return ((IFormatterConverter)this).ToBoolean (value);
         return base.Convert (value, type);
      }

      Boolean
      IFormatterConverter.ToBoolean (object value)
      {
         if (value is Boolean)
            return ((Boolean) value);
         
         if (value == null)
            throw new ArgumentNullException ("value");
         
         return DeserializeBooleanString (value.ToString ());
      }
   }

   /// <summary>
   ///   Returns a System.Type for the specified type name and namespace.
   /// </summary>
   /// <param name="typename">The name of the type.</param>
   /// <param name="namespace">The namespace of the type.</param>
   /// <returns></returns>
   protected internal Type
   GetObjectType (string typename, string @namespace, string genericArguments)
   {
      string fullname;

      //fullname = string.IsNullOrEmpty (@namespace)
      //               ? typename
      //               : @namespace + "." + typename;


      string []  nameParts = new string [6];
      int        part = 0;

      if (string.IsNullOrEmpty (@namespace))
      {
         nameParts [part++] = typename;
      }
      else
      {
         nameParts [part++] = @namespace;
         nameParts [part++] = ".";
         nameParts [part++] = typename;
      }

      if (!string.IsNullOrEmpty (genericArguments))
      {
         nameParts [part++] = "[";
         nameParts [part++] = genericArguments;
         nameParts [part++] = "]";
      }

      fullname = string.Concat (nameParts);
                     
      return GetObjectType (fullname);
   }

   /// <summary>
   ///   Returns a System.Type for the specified type name.
   /// </summary>
   /// <param name="fullname">The namespace-quallified object name.</param>
   /// <returns>The located Type or null.</returns>
   protected internal Type
   GetObjectType (string fullname)
   {
      Type objtype;

      if (fullname == null)
         return null;
      
      // First, check to see if we've already resolved this type.
      objtype = m_typeCache [fullname] as Type;
      
      if (objtype != null)
         return objtype;

      TypeNameToken  token;
      Token          next; // discard out parameter

      // Parse the fullname into a tree.
      token = ParseNamespaceQualifiedTypeName (fullname);

      if (token.ToString () != fullname)
         throw new ArgumentException (
            @"Invalid type name specified: """ + fullname
            + @""" or assertion failure.", "fullname");

      objtype = GetObjectType (token, out next);
      return objtype;
   }

   protected internal Type
   GetObjectType (TypeNameToken token, out Token next)
   {
      Type []  genericArguments;
      string   typename;
      int      arrayDimensions;
      
      Type objtype;

      if (token == null)
      {
         next = null;
         return null;
      }
      
      string  fullname = token.GetFullName ();
      
      GetObjectType (token, out genericArguments, out typename,
                     out arrayDimensions, out next);

      // First, check to see if we've already resolved this type.
      // Important!  Do this after advancing the 'next' token.
      objtype = m_typeCache [fullname] as Type;
      
      if (objtype != null)
         return objtype;

      // Return the first matching type.
      foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
      {
         Type type;

         type = ass.GetType (typename);
         if (type == null)
            continue;
         if (genericArguments != null)
            type = type.MakeGenericType (genericArguments);
         if (arrayDimensions == 1)
            type = type.MakeArrayType ();  // Make vector.
         else if (arrayDimensions > 1)
            type = type.MakeArrayType (arrayDimensions);  // Make array.

         m_typeCache.Add (fullname, type);
         return type;
      }
      
      return null;
   }

   /// <summary>
   ///   Returns a System.Type for the specified type name.
   /// </summary>
   /// <param name="fullname">The namespace-quallified object name.</param>
   /// <returns>The located Type or null.</returns>
   protected internal void
   GetObjectType (TypeNameToken token, out Type [] genericArguments,
                  out string typename, out int arrayDimensions, out Token next)
   {
      if (token == null)
         throw new ArgumentNullException ("token");


      if (token.Right is GenericArgumentBeginToken)
      {
         GenericArgumentBeginToken t = (GenericArgumentBeginToken) token.Right;
         System.Collections.Generic.List<Type> genArgs;
         genArgs = new System.Collections.Generic.List<Type> ();
         
         typename = token.GetName ();
         arrayDimensions = 0;
         for (next = t.Right;
              next is TypeNameToken;
              /* the internals advance 'next' */)
         {
            TypeNameToken  trt = (TypeNameToken) next;
            Type           tArg = GetObjectType (trt, out next);
            
            if (tArg == null)
               throw new SerializationException (
                  "Object of type " + trt.GetFullName () + " could not be " +
                  "deserialized.  The type could not be found in any " +
                  "assembly loaded in this AppDomain.  Is its assembly loaded?");
            genArgs.Add (tArg);
            
            if (next is GenericArgumentDelimeterToken)
            {
               next = next.Right;
            }
         }
         genericArguments = genArgs.ToArray ();
         if (!(next is GenericArgumentEndToken))
            throw new ArgumentException (
               "Invalid AST: Expected GenericArgumentEndToken.");
         next = next.Right;
         if (next is ArrayToken)
         {
            arrayDimensions = ((ArrayToken) next).Dimensions;
            next = next.Right;
         }
      }
      else
      {
         // Typename includes the array dimensions.
         typename = token.GetFullName ();
         genericArguments = null;
         arrayDimensions = 0;
         next = token.Right;
      
         if (next is ArrayToken)
         {
            // Typename includes the array dimensions.
            // So we don't need this code:
            // // arrayDimensions = ((ArrayToken) next).Dimensions;
            next = next.Right;
         }
      }
   }
   

   /// <summary>
   ///   Indicates whether a type implements an interface.
   /// </summary>
   /// <param name="type">A type</param>
   /// <param name="intrface">An interface</param>
   /// <returns>
   ///   A value indicating whether the type implements the interface.
   /// </returns>
   private bool
   IsTypeImplementing (Type type, Type intrface)
   {
      // "interface" is a keyword, so misspell.
      return null != type.GetInterface (intrface.ToString ());
   }
   
   /// <summary>
   ///   Constructs an object of the specified type with the specified
   ///   member values.
   /// </summary>
   /// <param name="objType">The type to construct.</param>
   /// <param name="xmlfields">The values for object members.</param>
   /// <returns>An object of the specified type.</returns>
   /// <remarks>
   ///   Supports Serializable and ISerializable objects.
   ///   Does not support SurrogateSelectors.
   /// </remarks>
   private object
   ConstructObject (Type objType, Hashtable xmlfields)
   {
      if (IsTypeImplementing (objType, typeof (ISerializable)))
      {
         return ConstructISerializableObject (objType, xmlfields);
      }
      else
      {
         return ConstructSerializableObject (objType, xmlfields);
      }
   }

   /// <summary>
   ///   Constructs an object of the specified type with the specified
   ///   member values.
   /// </summary>
   /// <param name="objType">The type to construct.</param>
   /// <param name="xmlfields">The values for object members.</param>
   /// <returns>An object of the specified type.</returns>
   /// <remarks>
   ///   Supports Serializable and ISerializable objects.
   ///   Does not support SurrogateSelectors.
   /// </remarks>
   private object
   ConstructISerializableObject (Type objType, Hashtable xmlfields)
   {
      // Use reflection to instantiate the type, supplying the
      // SerializationInfo and StreamingContext.
      // Catch expected exceptions to provide better information.
      try
      {
         ConstructorInfo    ctor;
         SerializationInfo  info;

         // Apparently the deserialization .ctor need not be public
         // (like in System.Collections.Hashtable). -JRS  2006-03-24
         ctor = objType.GetConstructor (FLAGS, null, new Type [] {
                                          typeof (SerializationInfo),
                                          typeof (StreamingContext)}, null);
         
         // The ISerializable interface cannot enforce a standard
         // deserialization constructor will exist, so if it does not,
         // attempt to use the non serializable method.
         if (ctor == null)
         {
            System.Diagnostics.Debug.WriteLine (
               "Could not find .ctor for ISerializable type: " +
               objType.FullName);
            return ConstructSerializableObject (objType, xmlfields);
         }
         
         // Populate the SerializationInfo object to pass into the constructor.
         info = new SerializationInfo (objType, XmlFormatterConverter.Instance);
         foreach (DictionaryEntry entry in xmlfields)
         {
            info.AddValue (entry.Key.ToString (), entry.Value);
         }

         return ctor.Invoke (new object [] {info, m_context});
      }
      catch (MethodAccessException mae)
      {
         // The caller does not have permission to execute this constructor.
         throw new SerializationException (
            "Object of type " + objType + " could not be deserialized.  " +
            "No construction permission.", mae);
      }
      catch (ArgumentException ae)
      {
         // No matching constructor was found.
         throw new SerializationException (
            "Object of type " + objType + " could not be deserialized.  " +
            "Deserialization constructor did accept parameters.", ae);
      }
      catch (TargetInvocationException tie)
      {
         // The constructor threw an exception.
         throw new SerializationException (
            "Object of type " + objType + " could not be deserialized.  " +
            "The constructor threw.", tie);
      }
      catch (TargetParameterCountException tpce)
      {
         // The wrong number of parameters was passed.
         throw new SerializationException (
            "Object of type " + objType + " could not be deserialized.  " +
            "The wrong number of parameters was passed.", tpce);
      }
   }

   /// <summary>
   ///   Constructs an object of the specified type with the specified
   ///   member values.
   /// </summary>
   /// <param name="objType">The type to construct.</param>
   /// <param name="xmlfields">The values for object members.</param>
   /// <returns>An object of the specified type.</returns>
   /// <remarks>
   ///   Supports Serializable and ISerializable objects.
   ///   Does not support SurrogateSelectors.
   /// </remarks>
   private object
   ConstructSerializableObject (Type objType, Hashtable xmlfields)
   {
      // Use the FormatterServices to instantiate and populate the object.
      object         ret = null;
      object []      data;
      MemberInfo []  members;
      
      try
      {
         members = FormatterServices.GetSerializableMembers (objType, 
                                                             m_context);
      }
      catch (Exception e)
      {
         throw new SerializationException (string.Format (
            "Cannot get members of {0}.",
            objType == null ? "null" : objType.FullName), e);
      }

      // Set up parallel array array containing values.
      data = new object [members.Length];
      
      // Populate the object with data from xmlfields where the
      // xml name matches the name of a serializable member.
      for (int i = 0; i < members.Length; i++)
      {
         string     mem;
         FieldInfo  field;
         Type       fieldtype;
         object     xmlfield;
         
         // Get the member name and type.
         field = members [i] as FieldInfo;
         mem = members [i].Name;
         
         xmlfield = xmlfields [mem];

         // Use the object and member types to determine
         // how to process the xmlfields.
         if (xmlfield == null)
         {
            // Do nothing because data [i] is already null.
            continue;
         }
         else if (field == null)
         {
            // Process non-field members.  Events???
            data [i] = xmlfield;
         }
         else if (xmlfield is Boolean)
         {
            // Process bool members.
            data [i] = (Boolean) xmlfield;
            continue;
         }
         else if (objType == typeof (string))
         {
            // Process string members.
            data [i] = xmlfield;
            continue;
         }
         
         fieldtype = field.FieldType;

         // Check for Nullable<>
         if (fieldtype.IsGenericType && 
            fieldtype.GetGenericTypeDefinition () == typeof(Nullable<>))
         {
            // This is Nullable<Nomething>.  Since we already know
            // the value of the xmlfield is NOT null, 
            // we may ignore the fact that the object is nullable
            // for some of the following comparisons.
            // Let's pretend it's "Something" instead of "Nullable<Something>".
            fieldtype = fieldtype.GetGenericArguments () [0];
         }
         
         if (fieldtype == typeof (String))
         {
            // Process string members.
            data [i] = xmlfield;
         }
         else if (fieldtype == typeof (Boolean))
         {
            // Process Boolean members.
            string  s = xmlfield.ToString ();
            if (s != "")
               data [i] = DeserializeBooleanString (s);
         }
         else if (fieldtype == typeof (DateTime))
         {
            // Process DateTime members.
            string  s = xmlfield.ToString ();
            if (s != "")
               data [i] = DateTime.Parse (s);
         }
         else if (fieldtype == typeof (Decimal))
         {
            // Process Decimal members.
            string  s = xmlfield.ToString ();
            if (s != "")
               data [i] = ParseDecimal (s);
         }
         else if (fieldtype == typeof (Guid))
         {
            // Process Guid members.
            string  s = xmlfield.ToString ();
            if (s != "")
               data [i] = new Guid (s);
         }
         else if (fieldtype == typeof (Object))
         {
            // Process explicitly object members.
            data [i] = xmlfield;
         }
         else if (fieldtype.IsArray)
         {
            // Process array members before primitives.
            data [i] = (Array) xmlfield;
         }
         else if (fieldtype.IsPrimitive)
         {
            // Process primitive members.
            string  s = xmlfield.ToString ();
            if (s != "")
               data [i] = Convert.ChangeType (xmlfield, fieldtype);
         }
         else if (fieldtype.IsEnum)
         {
            // Process primitive members.
            string  s = xmlfield.ToString ();
            if (s != "")
               data [i] = Enum.Parse (fieldtype, xmlfield.ToString (), true);
         }
         else if (fieldtype.IsInterface)
         {
            // Process complex members.
            data [i] = xmlfield;
         }
         else if (fieldtype.IsAssignableFrom (xmlfield.GetType ()))
         {
            // The runtime says we can do a direct assignment.
            data [i] = xmlfield;
         }
         else
         {
            // Last trench effort to change the type.
            data [i] = Convert.ChangeType (xmlfield, fieldtype);
         }
      }

      // Instantiate object and set member data.
      try
      {
         ret = FormatterServices.GetUninitializedObject (objType);
         FormatterServices.PopulateObjectMembers (ret, members, data);
      }
      catch (Exception e)
      {
         throw new SerializationException (
            string.Format (
               "FormatterServices failed to {0} object of type [{1}].",
               ret == null ? "create" : "populate", objType), e);
      }
      
      // Return the object.
      return ret;
   }
   
   private Decimal
   ParseDecimal (string s)
   {
      return Decimal.Parse (s, NumberStyles.Float);
   }
   
   private Double
   ParseDouble (string s)
   {
      return Double.Parse (s, NumberStyles.Float);
   }
   
   private Single
   ParseSingle (string s)
   {
      return Single.Parse (s, NumberStyles.Float);
   }
   
   private static bool
   DeserializeBooleanString (string val)
   {
      if (val == "0")
         return false;
      if (val == "-1")
         return true;
      return Boolean.Parse (val);
   }
   
   /// <summary>
   ///   Creates a SerializationException with the specified message.
   /// </summary>
   private SerializationException
   CreateException (string message, int line, int pos)
   {
      return CreateException (message, line, pos, null);
   }
   
   /// <summary>
   ///   Creates a SerializationException with the specified message.
   /// </summary>
   private SerializationException
   CreateException (string msg, int line, int pos, Exception inner)
   {
      const string FMT = "{0}\nAdded information: ({1},{2}).";
      return new SerializationException (string.Format (FMT, msg, line, pos),
                                         inner);
   }
   
   /// <summary>
   ///   Creates a SerializationException with the specified message.
   /// </summary>
   private SerializationException
   CreateException (string msg, XmlTextReader rdr)
   {
      return CreateException (msg, rdr, null);
   }
   
   /// <summary>
   ///   Creates a SerializationException with the specified message.
   /// </summary>
   private SerializationException
   CreateException (string msg, XmlTextReader rdr, Exception inner)
   {
      const string FMT = "{0}\nAdded information: ({1},{2},{3}).";
      
      string  s;
      s = string.Format (FMT, msg, rdr.LineNumber, rdr.LinePosition, rdr.Name);

      return new SerializationException (s, inner);
   }
   
   // Attribute name constants.
   private const string  ATTR_NS = "_NS";
   private const string  ATTR_GA = "_GA";
   private const string  ATTR_ARRAY_TYPE = "_ATyp";
   private const string  ATTR_ARRAY_CAPACITY = "_ACap";
   
   // Element name constants.
   private const string  ELEM_NULL = "_Null";

   // Prefix constants.
   private const string  PREFIX_TYPEDLIST = "TypedList:";  // Panther.Common.TypedList
   private const string  PREFIX_LIST = "List:"; // System.Collections.Generic.List
   
   // String 
   private const string DELIMITER_FULLY_QUALIFIED_TYPE_NAME = "+";
   private const string DELIMITER_NAMESPACE = ".";
   
   private const BindingFlags  FLAGS = BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Instance;
   
   // Cyclic object graph behavior.
   // If asserted, the serialization function will support cyclic object graphs.
   // When the serializer reaches a node twice in the stack, instead of
   // the default behavior of throwing, it will inserting the obj.ToString ()
   // instead of the object itself.  If formatted this way, deserialization may
   // not be possible.
   // Note: Constructors default this value to true;
   private bool  m_throwOnCyclic;
   
   // Serialization document.
   private XmlDocument  m_doc;

   // Maps a namespace.type string to the System.Type.
   private Hashtable  m_typeCache;
   
   // Contain IFormatter property fields.
   private SerializationBinder  m_binder;
   private StreamingContext     m_context;
   private ISurrogateSelector   m_selector;
   
   #region Class Definition : TypedList

   #warning remove typedlist.

   /// <summary>
   ///   Provides a way to store any objects of the same
   ///   type in a list.
   /// </summary>
   /// <remarks>
   ///   Upon creation of a TypedList, the caller
   ///   must provide the enclosed type of the list.  All objects
   ///   added or inserted into this list must be of type EnclosedType
   ///   or derive from EnclosedType.  Once the list is created, the 
   ///   enclosed type cannot be changed. Also, 
   ///   duplicate entries are allowed.  Null entries are not allowed.
   /// </remarks>
   [Serializable ()]
   private sealed class TypedList : IList, IEnumerable, ISerializable
   {
      /// <summary>
      ///   Use this constructor to create a TypedList with a
      ///   default capacity.
      /// </summary>
      /// <param name="enclosedType">The enclosed type.</param>
      public TypedList (Type enclosedType)
      {
         m_type = enclosedType;
         m_list = new ArrayList ();
      }

      /// <summary>
      ///   Use this constructor to create a TypedList with the
      ///   given capacity.
      /// </summary>
      /// <param name="enclosedType">The enclosed type.</param>
      /// <param name="capacity">Capacity of the list.</param>
      public TypedList (Type enclosedType, int capacity)
      {
         m_type = enclosedType;
         m_list = new ArrayList (capacity);
      }

      /// <summary>
      ///   Use this constructor to create a TypedList containing
      ///   all of the items in the ICollection.
      /// </summary>
      /// <remarks>
      ///   Throws ArgumentException if any of the items in the
      ///   ICollection are not of type EnclosedType.
      ///   Throws ArgumentNullException if any of the items in the
      ///   ICollection are null.
      /// </remarks>
      /// <param name="enclosedType"></param>
      /// <param name="items"></param>
      public TypedList (Type enclosedType, ICollection items)
      {
         m_type = enclosedType;
         ValidateType (items);
         m_list = new ArrayList (items);
      }
      
      /// <summary>
      ///   Used by a formatter to construct an object that had been serialized.
      /// </summary>
      /// <param name="info"></param>
      /// <param name="context"></param>
      public TypedList (SerializationInfo info, StreamingContext context)
      {
         // The name of the enclosed type.
         string  typename = info.GetString ("Type");

         // The name of the type to get out of the serialization info.
         string  arrayname = typename + "[]";

         // The enclosed type.
         Type  listtype = null;

         foreach (System.Reflection.Assembly ass
                  in AppDomain.CurrentDomain.GetAssemblies ())
         {
            listtype = ass.GetType (arrayname);

            if (listtype != null)
            {
               // Get the enclosed type from the same assembly.
               m_type = ass.GetType (typename);
               break;
            }
         }
         
         if (listtype == null)
         {
            throw new TypeLoadException (
               "Cannot construct TypedList of type: " + typename + ".");
         }
         else
         {
            m_list = new ArrayList (
               (IList) info.GetValue ("List", listtype));
         }
      }

      /// <summary>
      ///   Provides serialization information to formatters.
      /// </summary>
      /// <param name="info"></param>
      /// <param name="context"></param>
      public void
      GetObjectData (SerializationInfo info, StreamingContext context)
      {
         info.AddValue ("Type", m_type.FullName);
         info.AddValue ("List", m_list.ToArray (m_type));
      }

      /// <summary>
      ///   Copies the contents of the Panther.Common.ArrayList to an object array.
      /// </summary>
      public object []
      ToArray ()
      {
         return m_list.ToArray ();
      }

      /// <summary>
      ///   Copies the contents of the Panther.Common.ArrayList
      ///   to an array of the specified type.
      /// </summary>
      public Array
      ToArray (Type type)
      {
         return m_list.ToArray (type);
      }

      /// <summary>
      ///   Appends an item to the end of the list.
      /// </summary>
      /// <param name="item">The item to append.</param>
      /// <returns>
      ///   Returns the index indicating where the item was added.
      /// </returns>
      public int 
      Add (object item)
      {
         ValidateType (item);
         return m_list.Add (item);
      }

      /// <summary>
      ///   Removes all items from the list.
      /// </summary>
      public void 
      Clear ()
      {
         m_list.Clear ();
      }

      /// <summary>
      ///   Indicates whether or not an item is in the list.
      /// </summary>
      /// <param name="item">The item to find.</param>
      /// <returns>
      ///   Returns true if the item is in the list, otherwise returns false.
      /// </returns>
      public bool
      Contains (object item)
      {
         return m_list.Contains (item);
      }

      /// <summary>
      ///   Finds the index of a particular item.
      /// </summary>
      /// <param name="item">The item to find.</param>
      /// <returns>
      ///   Returns the index of the first occurrence of the item.
      /// </returns>
      public int 
      IndexOf (object item)
      {
         return m_list.IndexOf (item);
      }

      /// <summary>
      ///   Inserts an item at a specific index.
      /// </summary>
      /// <param name="index">The location to insert the item.</param>
      /// <param name="item">The item to insert.</param>
      /// <remarks>
      ///   The value of index must be less than or equal to the
      ///   number of items currently in the list.
      /// </remarks>
      public void
      Insert (int index, object item)
      {
         ValidateType (item);
         m_list.Insert (index, item);
      }

      /// <summary>
      ///   Removes the first occurrence of the item.
      /// </summary>
      /// <param name="item">The item to remove.</param>
      /// <remarks>
      ///   An exception is NOT thrown if the item does not exist in the list.
      /// </remarks>
      public void 
      Remove (object item)
      {
         m_list.Remove (item);
      }

      /// <summary>
      ///   Removes an item at a specific index.
      /// </summary>
      /// <param name="index"></param>
      /// <remarks>
      ///   The index must be strictly less than the number of items 
      ///   currently in the list.
      /// </remarks>
      public void
      RemoveAt (int index)
      {
         m_list.RemoveAt (index);
      }

      /// <summary>
      ///   Copies this list to the given array.
      /// </summary>
      /// <param name="array">The array to copy to.</param>
      /// <param name="index">Indicates where to start copying.</param>
      public void 
      CopyTo (Array array, int index)
      {
         m_list.CopyTo (array, index);
      }

      /// <returns>An enumerator for this list.</returns>
      public IEnumerator 
      GetEnumerator ()
      {
         return m_list.GetEnumerator ();
      }

      /// <summary>
      ///   The list is never a fixed sized.
      /// </summary>
      public bool IsFixedSize
      {
         get
         {
            return false;
         }
      }

      /// <summary>
      ///   The list is never read-only.
      /// </summary>
      public bool IsReadOnly
      {
         get
         {
            return false;
         }
      }

      /// <summary>
      ///   Indexer for the list.
      /// </summary>
      public object
      this [int index]
      {
         get
         {
            return m_list [index];
         }
         set
         {
            ValidateType (value);
            m_list [index] = value;
         }
      }

      /// <summary>
      ///   Number of items in the list.
      /// </summary>
      public int Count
      {
         get
         {
            return m_list.Count;
         }
      }

      /// <summary>
      ///   The list is not thread-safe.
      /// </summary>
      public bool IsSynchronized
      {
         get
         {
            return false;
         }
      }

      /// <summary>
      ///   Since this list is not thread-safe, null is returned.
      /// </summary>
      public object SyncRoot
      {
         get
         {
            return null;
         }
      }

      /// <summary>
      ///   The enclosed type of this list.  
      /// </summary>
      /// <remarks>
      ///   All items added to this list must be of this type or must 
      ///   derive from this type.
      /// </remarks>
      public Type EnclosedType
      {
         get
         {
            return m_type;
         }
      }
      
      /// <summary>
      ///   A String that represents the current Object.
      /// </summary>
      /// <returns></returns>
      public override string
      ToString ()
      {
         return "{TypedList: " + m_type.FullName + " [" + m_list.Count + "]}";
      }

      /// <summary>
      ///   Validates that all items of the ICollection can be 
      ///   added to this list.
      /// </summary>
      /// <param name="items">The collection to validate.</param>
      private void 
      ValidateType (ICollection items)
      {
         if (items == null)
         {
            string message = "ICollection cannot be null.";
            throw new ArgumentNullException ("items", message);
         }

         foreach (object item in items)
         {
            ValidateType (item);
         }
      }

      /// <summary>
      ///   Validates that the item can be added to this list.
      /// </summary>
      /// <param name="item"></param>
      /// <remarks>
      ///   Throws ArgumentNullException if item is null.  Throws
      ///   ArgumentException if the item's type is not the enclosed type or
      ///   the item's type does not derive from the enclosed type.
      /// </remarks>
      private void 
      ValidateType (object item)
      {
         if (item == null)
         {
            throw new ArgumentNullException ("item", "Item cannot be null.");
         }

         if (!m_type.IsAssignableFrom (item.GetType ()))
         {
            throw new ArgumentException ("Only items of type \"" + 
                                         m_type.FullName + 
                                         "\" or items derived from type \"" +
                                         m_type.FullName + 
                                         "\" can be added to this list.", "item");
         }
      }

      // Enclosed type.
      [NonSerialized ()]
      private Type  m_type;

      // Wrapped ArrayList.
      [NonSerialized ()]
      private ArrayList  m_list;
   }

   #endregion
}

#endregion

#region Class Definition : XmlFormatter

partial class XmlFormatter
{
   // Lexical Grammer for Type names used by XmlFormatter:
   // Tokens:
   //   Name  (namespace or type name components)
   //   .     (namespace delimeter)
   //   +     (nested type delimeter)
   //   [     (generic arguments or array declaration begining)
   //   ,     (generic arguments or array declaration delimeter)
   //   ]     (generic arguments or array declaration ending)
   //
   // NS:     (namespace component)
   //         Ex: "System"
   //   :=Name
   //
   // TN:     (type name component)
   //         Ex: "Nullable`1"
   //   :=Name
   //
   // NSQTNE: (namespace-qualified type name entry in generic argument list)
   //         Ex: ",System.Int32"
   //   :=,NSQTN NSQTNE
   //   :=
   //
   // NSQTN:  (namespace-qualified type name)
   //         Ex: "System.Nullable`1[System.In32][]"
   //   :=NS.NSQTN
   //   :=TQTN
   //
   // TQTN:   (type-qualified type name)
   //         Ex: "ListView+Item"
   //   :=TN+TQTN
   //   :=TN GA A
   //
   // GA:     (generic type arguments)
   //         Ex: "[System.Int32,System.String]"
   //   :=[NSQTN NSQTNE]
   //   :=
   //
   // A:      (array type declaration)
   //         Ex: "[]", "[,]"
   //   :=[AE]
   //   :=
   //
   // AE:     (array rank declaration)
   //         Ex: ",,,"
   //   :=,AE
   //   :=
   //
   // Known limitations:
   // 2007-01-16: This grammer does not support arrays of arrays,
   //             but neither does the XmlFormatter.  To add this support,
   //             Add "A := A A", making sure the associativity mirrors
   //             that of type definitions in the CLR (esp. Type.Load ()).
   //

   /// <summary>
   /// Breaks text into pieces.
   /// </summary>
   /// <param name="text"></param>
   /// <returns></returns>
   internal static string []
   Lex (string text)
   {
      List<string>  tokens = new List<string> ();
      
      int marker = 0;
      int pos;
      
      for (pos = 0; pos < text.Length; pos++)
      {
         char cur = text [pos];
         
         switch (cur)
         {
         case '[':
         case ']':
         case ',':
         case '.':
         case '+':
            if (marker != pos)
            {
               string  before = text.Substring (marker, pos - marker);
               tokens.Add (before);
            }
            tokens.Add (cur.ToString ());
            marker = pos + 1;
            break;
         }
      }
      
      if (marker != pos)
      {
         string  before = text.Substring (marker, pos - marker);
         tokens.Add (before);
      }
      
      return tokens.ToArray ();
   }
   
   /// <summary>
   /// Lexes and parses a namespace-qualified type name.
   /// </summary>
   /// <param name="fullname"></param>
   /// <returns></returns>
   internal static TypeNameToken
   ParseNamespaceQualifiedTypeName (string fullname)
   {
      string []                      tokenStrings;
      ArrayCursorEnumerator<string>  tokens;
      TypeNameToken                  token;
      
      tokenStrings = Lex (fullname);
      tokens = new ArrayCursorEnumerator<string> (tokenStrings);
      if (tokens.MoveNext ())
         token = ParseNSQTN (tokens, null);
      else
         throw new InvalidOperationException ("Assertion failure: no tokens.");
      
      return token;
   }

   /// <summary>
   /// Parses a namespace-qualified type name.
   /// </summary>
   /// <param name="tokens"></param>
   /// <param name="ns"></param>
   /// <returns></returns>
   internal static TypeNameToken
   ParseNSQTN (ArrayCursorEnumerator<string> tokens, Token ns)
   {
      Token  currentToken;
      switch (tokens.Next)
      {
      case NamespaceDelimeterToken.STRING:        // "."
         currentToken = new NamespaceToken (tokens.Current);
         tokens.MoveNext ();
         currentToken.AddRight (new NamespaceDelimeterToken (tokens.Current));
         currentToken.AddLeft (ns);
         tokens.MoveNext ();
         return ParseNSQTN (tokens, currentToken);
      case TypeNameDelimeterToken.STRING:         // "+"
      case GenericArgumentBeginToken.STRING:      // "["
      case GenericArgumentDelimeterToken.STRING:  // ","
      case GenericArgumentEndToken.STRING:        // "]"
      default:                                    // identifier
      case null:                                  // EndOfStream
         return ParseTQTN (tokens, ns);
      }
   }
   
   /// <summary>
   /// Parses generic argument delimeter followed by
   /// a namespace-qualified type name.
   /// </summary>
   /// <param name="tokens"></param>
   /// <returns></returns>
   internal static GenericArgumentDelimeterToken
   ParseNSQTNE (ArrayCursorEnumerator<string> tokens)
   {
      GenericArgumentDelimeterToken  currentToken;
      
      if (tokens.IsComplete)
         return null;
      
      if (tokens.Current != GenericArgumentDelimeterToken.STRING)
         return null;
      
      currentToken = new GenericArgumentDelimeterToken (tokens.Current);
      
      tokens.MoveNext ();
      
      currentToken.AddRight (ParseNSQTN (tokens, null));
      currentToken.AddRight (ParseNSQTNE (tokens));
      
      return currentToken;
   }
   
   /// <summary>
   /// Parses a type-qualified type name.
   /// </summary>
   /// <param name="tokens"></param>
   /// <param name="ns"></param>
   /// <returns></returns>
   internal static TypeNameToken
   ParseTQTN (ArrayCursorEnumerator<string> tokens, Token ns)
   {
      TypeNameToken  currentToken;
      switch (tokens.Next)
      {
      case TypeNameDelimeterToken.STRING:         // "+"
         currentToken = new TypeNameToken (tokens.Current);
         tokens.MoveNext ();
         currentToken.AddRight (new TypeNameDelimeterToken (tokens.Current));
         currentToken.AddLeft (ns);
         tokens.MoveNext ();
         return ParseTQTN (tokens, currentToken);
      case NamespaceDelimeterToken.STRING:        // "."
      case GenericArgumentBeginToken.STRING:      // "["
         currentToken = new TypeNameToken (tokens.Current);
         currentToken.AddLeft (ns);
         tokens.MoveNext ();
         currentToken.AddRight (ParseGA (tokens));
         currentToken.AddRight (ParseA (tokens));
         if (currentToken.Right == null)
            throw new InvalidOperationException ();
         return currentToken;
      
      case GenericArgumentDelimeterToken.STRING:  // ","
      case GenericArgumentEndToken.STRING:        // "]"
      case null:                                  // EndOfStream
         currentToken = new TypeNameToken (tokens.Current);
         tokens.MoveNext ();
         currentToken.AddLeft (ns);
         return currentToken;
      default:                                    // identifier
         throw new InvalidOperationException ();
      }
   }

   /// <summary>
   /// Parses generic arguments of a type.
   /// </summary>
   /// <param name="tokens"></param>
   /// <returns></returns>
   internal static GenericArgumentBeginToken
   ParseGA (ArrayCursorEnumerator<string> tokens)
   {
      GenericArgumentBeginToken  currentToken;

      if (tokens.IsComplete)
         return null;
      
      if (tokens.Current != GenericArgumentBeginToken.STRING)
         return null;
      
      switch (tokens.Next)
      {
      case TypeNameDelimeterToken.STRING:         // "+"
      case NamespaceDelimeterToken.STRING:        // "."
      case GenericArgumentBeginToken.STRING:      // "["
      case GenericArgumentDelimeterToken.STRING:  // ","
      case GenericArgumentEndToken.STRING:        // "]"
      case null:                                  // EndOfStream
         return null;
      default:                                    // identifier
         currentToken = new GenericArgumentBeginToken (tokens.Current);
         tokens.MoveNext ();
         currentToken.AddRight (ParseNSQTN (tokens, null));
         currentToken.AddRight (ParseNSQTNE (tokens));
         if (tokens.Current != GenericArgumentEndToken.STRING)
            throw new InvalidOperationException ();
         currentToken.AddRight (new GenericArgumentEndToken (tokens.Current));
         tokens.MoveNext ();
         return currentToken;
      }
   }
   
   /// <summary>
   /// Parses an array declaration.
   /// </summary>
   /// <param name="tokens"></param>
   /// <returns></returns>
   internal static ArrayToken
   ParseA (ArrayCursorEnumerator<string> tokens)
   {
      ArrayToken  currentToken;

      if (tokens.IsComplete)
         return null;
      
      if (tokens.Current != GenericArgumentBeginToken.STRING)
         return null;
      
      switch (tokens.Next)
      {
      case GenericArgumentDelimeterToken.STRING:  // ","
      case GenericArgumentEndToken.STRING:        // "]"
         currentToken = new ArrayToken ();
         tokens.MoveNext ();
         while (tokens.Current == GenericArgumentDelimeterToken.STRING)
         {
            currentToken.IncrementDimensions (1);
            tokens.MoveNext ();
         }
         if (tokens.Current == GenericArgumentEndToken.STRING)
         {
            // move past the end bracket.
            tokens.MoveNext ();
            return currentToken;
         }
         throw new InvalidOperationException (
            "Unexpected token in array declaration: " + tokens.Current);
      
      case TypeNameDelimeterToken.STRING:         // "+"
      case NamespaceDelimeterToken.STRING:        // "."
      case GenericArgumentBeginToken.STRING:      // "["
      case null:                                  // EndOfStream
      default:                                    // identifier
         return null;
      }
   }
   
   /// <summary>
   /// Base-class for type name tokens.
   /// </summary>
   protected internal class Token
   {
      public
      Token ()
      {
      }
      
      /// <param name="value">The textual representation of the token.</param>
      public
      Token (string value)
      {
         this.Value = value;
      }

      /// <param name="value">The textual representation of the token.</param>
      /// <param name="requiredValue">The string value must equal.</param>
      protected
      Token (string value, string requiredValue)
      {
         if (value != requiredValue)
            throw new ArgumentOutOfRangeException ("value", value,
               string.Format (@"{0} only accepts values of ""{1}"".",
                  GetType ().Name, requiredValue));
         this.Value = value;
      }
      

      public override string
      ToString ()
      {
         return Left + Value + Right;
      }
      
      public static Token
      AddLeft (Token addedTo, Token left)
      {
         if (addedTo == null)
            return left;
         if (left == null)
            return addedTo;
         Token  ret = addedTo;
         while (addedTo.Left != null)
            addedTo=addedTo.Left;
         addedTo.Left = left;
         return ret;
      }
      
      public static Token
      AddRight (Token addedTo, Token right)
      {
         if (addedTo == null)
            return right;
         if (right == null)
            return addedTo;
         Token  ret = addedTo;
         while (addedTo.Right != null)
            addedTo=addedTo.Right;
         addedTo.Right = right;
         return ret;
      }
      
      public Token
      AddLeft (Token left)
      {
         return AddLeft (this, left);
      }
      
      public Token
      AddRight (Token right)
      {
         return AddRight (this, right);
      }
      
      /// <summary>The textual representation of the token.</summary>
      public string  Value;
      /// <summary>The token on the left.</summary>
      public Token   Left;
      /// <summary>The token on the left.</summary>
      public Token   Right;
   }
   
   /// <summary>
   /// A component of a namespace name.
   /// </summary>
   protected internal class NamespaceToken : Token // Ex: "System"
   {
      public
      NamespaceToken (string value)
         : base (value)
      {
      }
   }
   
   /// <summary>
   /// A component of a type name.
   /// </summary>
   protected internal class TypeNameToken : Token // Ex: "Int32"
   {
      public
      TypeNameToken (string value)
         : base (value)
      {
      }
      
      public string
      GetName ()
      {
         System.Text.StringBuilder  name;
         name = new System.Text.StringBuilder ();
         
         // Recursivly determine the name and namespace of this type.
         AppendName (name, this);
         return name.ToString ();
      }
      
      internal static void
      AppendName (System.Text.StringBuilder name, TypeNameToken token)
      {
         if (token.Left is TypeNameToken)
         {
            // Recurse.
            AppendName (name, token.Left as TypeNameToken);
            name.Append (TypeNameDelimeterToken.STRING);
         }
         else if (token.Left is NamespaceToken)
         {
            AppendName (name, token.Left as NamespaceToken);
            name.Append (NamespaceDelimeterToken.STRING);
         }
         name.Append (token.Value);
      }
      
      internal static void
      AppendName (System.Text.StringBuilder name, NamespaceToken token)
      {
         if (token.Left is NamespaceToken)
         {
            // Recurse.
            AppendName (name, token.Left as NamespaceToken);
            name.Append (NamespaceDelimeterToken.STRING);
         }
         name.Append (token.Value);
      }

      internal string
      GetFullName ()
      {
         Token  next;
         System.Text.StringBuilder  name;
         name = new System.Text.StringBuilder ();
         
         // Recursivly determine the name and namespace of this type.
         AppendFullName (name, out next);

         return name.ToString ();
      }

      internal void
      AppendFullName (System.Text.StringBuilder name, out Token next)
      {
         // Recursivly determine the name and namespace of this type.
         AppendName (name, this);
         
         if (this.Right is GenericArgumentBeginToken)
         {
            // Recursivly determine the name and namespace of this type.
            ((GenericArgumentBeginToken)this.Right).AppendGenericArguments (
                                                      name, out next);
            if (next is ArrayToken)
            {
               // Recursivly determine the name and namespace of this type.
               name.Append (next.Value);
               next = next.Right;
            }
         }
         else if (this.Right is ArrayToken)
         {
            // Recursivly determine the name and namespace of this type.
            name.Append (this.Right.Value);
            next = this.Right.Right;
         }
         else
         {
            next = this.Right;
         }
      }
   }
   
   protected internal class NamespaceDelimeterToken : Token // Ex: "."
   {
      public const string  STRING = ".";
      public
      NamespaceDelimeterToken (string value)
         : base (value, STRING)
      {
      }
   }
   
   protected internal class TypeNameDelimeterToken : Token // "+"
   {
      public const string  STRING = "+";
      public
      TypeNameDelimeterToken (string value)
         : base (value, STRING)
      {
      }
   }
   
   protected internal class GenericArgumentBeginToken : Token // "["
   {
      public const string  STRING = "[";
      public
      GenericArgumentBeginToken (string value)
         : base (value, STRING)
      {
      }

      internal void
      AppendGenericArguments (System.Text.StringBuilder name, out Token next)
      {
         name.Append (this.Value);
         
         next = this; // Satisfy the out parameter.
         
         while (next.Right is TypeNameToken)
         {
            next = next.Right;
            ((TypeNameToken)next).AppendFullName (name, out next);
            //next = next.Right;
            if (next is GenericArgumentDelimeterToken)
            {
               name.Append (next.Value);
               //next = next.Right;
            }
            else if (next is GenericArgumentEndToken)
               break;
         }
         if (next is GenericArgumentEndToken)
         {
            name.Append (next.Value);
            next = next.Right;
         }
         else
         {
            throw new ArgumentException ("Missing End Generic Arguments");
         }
      }
   }
   
   protected internal class GenericArgumentDelimeterToken : Token // ","
   {
      public const string  STRING = ",";
      public
      GenericArgumentDelimeterToken (string value)
         : base (value, STRING)
      {
      }
   }

   protected internal class GenericArgumentEndToken : Token // "]"
   {
      public const string  STRING = "]";
      public
      GenericArgumentEndToken (string value)
         : base (value, STRING)
      {
      }
   }
   
   protected internal class ArrayToken : Token // "[]"
   {
      public const string  STRING = "[]";
      public
      ArrayToken ()
         : base (STRING)
      {
      }
      
      public int
      Dimensions
      {
         get
         {
            // "[]" = 1;
            // "[,]" = 2; etc...
            return Value.Length - 1;
         }
      }
      
      public void
      IncrementDimensions (int count)
      {
         // Insert a comma after the '['.
         Value = "[" + new string (',', count) + Value.Substring (1);
      }
   }
   
   protected internal class ArrayCursorEnumerator<T> : IEnumerator<T>
   {
      public
      ArrayCursorEnumerator (T [] array)
      {
         m_array = array;
         m_current = -1;
      }
      
      public T
      Current
      {
         get
         {
            if (m_current < 0)
               throw new InvalidOperationException (
                  "Call MoveNext before Current.");
            if (m_current >= m_array.Length)
               throw new InvalidOperationException (
                  "Enumeration is already complete.");
            return m_array [m_current];
         }
      }

      public T
      Next
      {
         get
         {
            if (m_current < 0)
               throw new InvalidOperationException (
                  "Call MoveNext before Next.");
            if (m_current+1 >= m_array.Length)
               return default(T);
            return m_array [m_current+1];
         }
      }

      public bool
      IsComplete
      {
         get
         {
            if (m_current >= m_array.Length)
               return true;;
            return false;
         }
      }

      public bool
      HasCurrent
      {
         get
         {
            if (m_current < 0)
               return false;
            if (m_current >= m_array.Length)
               return false;
            return true;
         }
      }

      public bool
      HasNext
      {
         get
         {
            if (m_current < 0)
               throw new InvalidOperationException ("Call MoveNext first.");
            return m_current+1 < m_array.Length;
         }
      }

      public void
      Dispose ()
      {
      }

      object
      System.Collections.IEnumerator.Current
      {
         get
         {
            return this.Current;
         }
      }

      public bool
      MoveNext ()
      {
         return ++m_current < m_array.Length;
      }

      public void 
      Reset ()
      {
         m_current = -1;
      }

      private T []  m_array;
      private int   m_current;
   }
}

#endregion
}
