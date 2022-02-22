using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Lucene
{
    class Program
    {
        static void Main(string[] args)
        {
            const LuceneVersion AppLunceneVersion = LuceneVersion.LUCENE_48;
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var indexPath = Path.Combine(basePath, "index");
            var dir = FSDirectory.Open(indexPath);
            var analyzer = new StandardAnalyzer(AppLunceneVersion);
            var indexConfig = new IndexWriterConfig(AppLunceneVersion, analyzer);
            var writer = new IndexWriter(dir, indexConfig);

            var pharase = new MultiPhraseQuery
            {
                new Term("favoritePhrase", "brown"),
                new Term("favoritePhrase", "fox")
            };

            var source = new
            {
                Name = "Kermit the Frog",
                FavoritePhrase = "The quick brown fox jumps over the lazy dog"
            };
            var doc = new Document
            {
                new StringField("name", source.Name, Field.Store.YES),
                new TextField("favoritePhrase", source.FavoritePhrase, Field.Store.YES)
            };

            writer.AddDocument(doc);
            writer.Flush(triggerMerge: false, applyAllDeletes: false);

            var reader = writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            var hits = searcher.Search(pharase, 20).ScoreDocs;

            Console.WriteLine($"{"Score",10}" +
                              $" {"Name",-15}" +
                              $" {"Favorite Phrase",-40}");

            foreach (var hit in hits)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                Console.WriteLine($"{hit.Score:f8}" +
                                  $" {foundDoc.Get("name"),-15}" +
                                  $" {foundDoc.Get("favoritePhrase"),-40}");
            }

            Console.ReadLine();
        }
    }
}
