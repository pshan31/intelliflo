using IntelliFloCore.Common;
using IntelliFloCore.Querying;
using LiteDB;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Payloads;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Query = Lucene.Net.Search.Query;

namespace IntelliFloCore_NetCore.Ingestion
{
    public static class FastIngestCommon
    {
        static IndexWriter writer;
        static int count = 0;

        const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
        static FastIngestCommon()
        {
            //Open the Directory using a Lucene Directory class
            string indexName = "intelliflo_ingest";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);

            // Load existing index or create new if it doesn't exist
            FSDirectory indexDir = FSDirectory.Open(new DirectoryInfo(indexPath));
            IndexWriterConfig indexConfig = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));
            indexConfig.OpenMode = OpenMode.CREATE_OR_APPEND;
            writer = new IndexWriter(indexDir, indexConfig);
        }

        public static void Mtd()
        {
            //Add three documents to the index
            Document doc = new Document();
            doc.Add(new TextField("title", "The Apache Software Foundation - The world's largest open source foundation.", Field.Store.YES));
            doc.Add(new StringField("domain", "www.apache.org/", Field.Store.YES));
            writer.AddDocument(doc);

            doc = new Document();
            doc.Add(new TextField("title", "Powerful open source search library for .NET", Field.Store.YES));
            doc.Add(new StringField("domain", "lucenenet.apache.org", Field.Store.YES));
            writer.AddDocument(doc);

            doc = new Document();
            doc.Add(new TextField("title", "Unique gifts made by small businesses in North Carolina.", Field.Store.YES));
            doc.Add(new StringField("domain", "www.giftoasis.com", Field.Store.YES));
            writer.AddDocument(doc);

            //Flush and commit the index data to the directory
            writer.Commit();

            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);

            Query query = new TermQuery(new Term("domain", "lucenenet.apache.org"));
            TopDocs topDocs = searcher.Search(query, n: 2);         //indicate we want the first 2 results


            int numMatchingDocs = topDocs.TotalHits;
            Document resultDoc = searcher.Doc(topDocs.ScoreDocs[0].Doc);  //read back first doc from results (ie 0 offset)
            string title = resultDoc.Get("title");

            Console.WriteLine($"Matching results: {topDocs.TotalHits}");
            Console.WriteLine($"Title of first result: {title}");
            reader.Dispose();
        }

        public static void IngestData(string message, Dictionary<string, string> pairs)
        {
            Document doc = new Document();
            doc.Add(new TextField("_rawmessage", message, Field.Store.YES));

            foreach (var item in pairs)
            {
                if (item.Key == "Timestamp")
                {
                    doc.Add(new Int64Field("Timestamp", long.Parse(item.Value), Field.Store.YES));
                }
                else if (item.Value.IsNumeric(out double converted))
                    doc.Add(new DoubleField(item.Key, converted, Field.Store.YES));
                else
                    doc.Add(new StringField(item.Key, item.Value, Field.Store.YES));
            }

            writer.AddDocument(doc);

            //// Commit changes periodically
            if (writer.NumDocs % 1000 == 0)
            {
                writer.Commit();
            }
        }

        public static List<Document> SearchData(string queryStr, long from, long to, bool hasSubQuery)
        {
            List<Document> res = new();
            // Open index reader
            using (var indexReader = writer.GetReader(true))
            {
                // Perform search operations (replace with actual search logic)

                // Open thread-safe index searcher
                var indexSearcher = new IndexSearcher(indexReader);

                Analyzer standardAnalyzer = new StandardAnalyzer(luceneVersion);
                // Example search query
                Lucene.Net.QueryParsers.Classic.QueryParser parser = new Lucene.Net.QueryParsers.Classic.QueryParser(luceneVersion, "_rawmessage", standardAnalyzer);
                // Example timestamp range query
                var timestampFrom = DateTime.Now.ToUniversalTime().Date; // Example: Start of today
                var timestampTo = DateTime.Now.ToUniversalTime(); // Example: Current time

                parser.AllowLeadingWildcard = true;
                // Construct a NumericRangeQuery for the timestamp field
                var timestampQuery = NumericRangeQuery.NewInt64Range("Timestamp", from, to, true, true);

                Query query = parser.Parse(queryStr);

                var booleanQuery = new BooleanQuery
{
    { timestampQuery, Occur.MUST }, // Documents must match the timestamp range
    { query, Occur.MUST } // Documents must also match the LogData:error condition
};

                TopDocs topDocs = indexSearcher.Search(booleanQuery, hasSubQuery ? int.MaxValue : 5000);

                Console.WriteLine($"Matching results: {topDocs.TotalHits}");

                for (int i = 0; i < topDocs.TotalHits; i++)
                {
                    try
                    {
                        //read back a doc from results
                        Document resultDoc = indexSearcher.Doc(topDocs.ScoreDocs[i].Doc);

                        res.Add(resultDoc);
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
            return res;
        }

        public static void Commit()
        {
            writer.Commit();
        }
    }
}
