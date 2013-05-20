using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lucene.Net;
using Lucene.Net.Documents;

namespace Sellars.Indexing
{
   public class IndexSession
   {
      private static readonly string [] STOP = {"a","and"};
      public static IndexSession CreateInMemorySession ()
      {
         var d = new Lucene.Net.Store.RAMDirectory ();

         var stopwords = 
            STOP.Aggregate (
               new System.Collections.Hashtable (),
               (h, word) => {h.Add (word,word);return h;});

         var a = 
            new Lucene.Net.Analysis.Standard.StandardAnalyzer (
               Lucene.Net.Util.Version.LUCENE_29, 
               stopwords);

         var writer = new Lucene.Net.Index.IndexWriter (d, a);
         var reader = writer.GetReader (); //Lucene.Net.Index.DirectoryReader.Open (d);

         return new IndexSession (d, a, reader, writer);
      }

      public string [] Fields
      {
         get
         {
            return m_fields ?? new string[0];
         }
         set
         {
            m_fields = value;
         }
      }

      public IndexSession (
         Lucene.Net.Store.Directory directory, Lucene.Net.Analysis.Analyzer analyzer,
         Lucene.Net.Index.IndexReader reader, Lucene.Net.Index.IndexWriter writer)
      {
         m_directory = directory;
         m_analyzer = analyzer;
         m_reader = reader;
         m_writer = writer;
      }

      public Lucene.Net.Documents.Document AddDocument (Dictionary<string,string> document)
      {
         Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document ();
         
         foreach (var kvp in document.Where (kvp => !string.IsNullOrEmpty (kvp.Value)))
         {
            doc.Add (
               new Lucene.Net.Documents.Field (
                  kvp.Key, 
                  kvp.Value,
                  Lucene.Net.Documents.Field.Store.YES,
                  Field.Index.ANALYZED));
         }

         m_writer.AddDocument (doc);
         m_writer.Commit ();
         return doc;
      }

      public IEnumerable<Lucene.Net.Documents.Document> Search (string query)
      {
         m_reader = Lucene.Net.Index.DirectoryReader.Open (m_directory, true);
         Lucene.Net.Search.IndexSearcher searcher = new Lucene.Net.Search.IndexSearcher (m_reader);
         var parser = new Lucene.Net.QueryParsers.MultiFieldQueryParser (Lucene.Net.Util.Version.LUCENE_29, Fields, m_analyzer);
         var queryObject = parser.Parse (query);
         
         var topFieldDocs = searcher.Search (queryObject, null, 10000);
         
         for (int i = 0; i < topFieldDocs.totalHits; i++)
         {
            yield return m_reader.Document ( topFieldDocs.scoreDocs[i].doc);
         }
      }

      private Lucene.Net.Store.Directory m_directory;
      private Lucene.Net.Analysis.Analyzer m_analyzer;
      private Lucene.Net.Index.IndexReader m_reader;
      private Lucene.Net.Index.IndexWriter m_writer;
      private string [] m_fields;
   }
}
