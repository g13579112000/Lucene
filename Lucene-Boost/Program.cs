using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

var _dir = new DirectoryInfo("LuceneDocument");
if (!File.Exists(_dir.FullName))
{
    System.IO.Directory.CreateDirectory(_dir.FullName);
}
var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);
CreateIndex(GetProductsInformation(), _dir, analyzer);

while (true)
{
    Console.Write("請輸入欲查詢字串 :");
    var searchValue = Console.ReadLine();
    Search(searchValue, _dir, analyzer);

    Console.Write("請輸入欲查詢字串 :");
    searchValue = Console.ReadLine();
    SearchWithBoost(searchValue, _dir, analyzer);

    Console.Write("請輸入欲更新Name索引 :"); 
     var updateKey = Console.ReadLine();
    Update(updateKey, GetUpdateProductsInformation(), _dir, analyzer);

    Console.Write("請輸入欲查詢字串 :");
    searchValue = Console.ReadLine();
    Search(searchValue, _dir, analyzer);

    Console.Write("請輸入欲刪除Name索引 :");
    var deleteKey = Console.ReadLine();
    Delete(deleteKey, _dir, analyzer);
}

void Update(string key, List<Product> information, DirectoryInfo dir, StandardAnalyzer analyzer)
{
    using( var directory = FSDirectory.Open(dir))
    {
        using(var indexWriter = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.LIMITED))
        {
            foreach (var index in information)
            {
                var document = new Document();
                document.Add(new Field("Id", index.Id.ToString(), Field.Store.YES, Field.Index.NO));
                document.Add(new Field("Name", index.Name, Field.Store.YES, Field.Index.ANALYZED));
                document.Add(new Field("Description", index.Description, Field.Store.YES, Field.Index.ANALYZED));
                indexWriter.UpdateDocument(new Term("Name", key) ,document);
            }
            indexWriter.Commit();
        }
    }
}

void Delete(string key, DirectoryInfo dir, StandardAnalyzer analyzer)
{
    using (var directory = FSDirectory.Open(dir))
    {
        using (var indexWriter = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.LIMITED))
        {
            indexWriter.DeleteDocuments(new Term("Name", key));
            indexWriter.Optimize();
            indexWriter.Commit();
        }
    }
}

void CreateIndex(List<Product> information, DirectoryInfo dir, StandardAnalyzer analyzer)
{
    using (var directory = FSDirectory.Open(dir))
    {
        using (var indexWriter = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED))
        {
            foreach (var index in information)
            {
                var document = new Document();
                document.Add(new Field("Id", index.Id.ToString(), Field.Store.YES, Field.Index.NO));
                document.Add(new Field("Name", index.Name, Field.Store.YES, Field.Index.ANALYZED));
                document.Add(new Field("Description", index.Description, Field.Store.YES, Field.Index.ANALYZED));

                indexWriter.AddDocument(document);
            }
            indexWriter.Optimize();
            indexWriter.Commit();
        }
    }
}

void CreateIndexWithBoost(List<Product> information, DirectoryInfo dir, StandardAnalyzer analyzer)
{
    using (var directory = FSDirectory.Open(dir))
    {
        using (var indexWriter = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED))
        {
            foreach (var index in information)
            {
                var document = new Document();
                document.Add(new Field("Id", index.Id.ToString(), Field.Store.YES, Field.Index.NO));
                document.Add(new Field("Name", index.Name, Field.Store.YES, Field.Index.ANALYZED));
                document.Add(new Field("Description", index.Description, Field.Store.YES, Field.Index.ANALYZED));
                document.GetField("Name").Boost = 1.5F;
                document.GetField("Description").Boost = 0.5F;

                indexWriter.AddDocument(document);
            }
            indexWriter.Optimize();
            indexWriter.Commit();
        }
    }
}

void Search(string searchValue, DirectoryInfo dir, StandardAnalyzer analyzer)
{
    using (var directory = FSDirectory.Open(_dir))
    {
        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, "Description", analyzer).Parse(searchValue);
        using (var indexSearcher = new IndexSearcher(directory))
        {
            var queryLimit = 20;
            var hits = indexSearcher.Search(parser, queryLimit);
            if (!hits.ScoreDocs.Any())
            {
                Console.WriteLine("查無相關結果。");
                return;
            }
            Document doc;
            foreach (var hit in hits.ScoreDocs)
            {
                doc = indexSearcher.Doc(hit.Doc);
                Console.WriteLine("Score :" + hit.Score + ", Id :" + doc.Get("Id") + ", Name :" + doc.Get("Name") + ", Description :" + doc.Get("Description"));
            }
        }
    }
}

void SearchWithBoost(string searchValue, DirectoryInfo dir, StandardAnalyzer analyzer)
{
    using (var directory = FSDirectory.Open(_dir))
    {
        using (var indexSearcher = new IndexSearcher(directory))
        {
            var query = new QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, "Name", analyzer).Parse(searchValue);
            var query2 = new QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, "Description", analyzer).Parse(searchValue);

            query.Boost = 2.0F;
            query2.Boost = 0.5F;

            BooleanQuery booleanQuery = new BooleanQuery();
            booleanQuery.Add(query, Occur.SHOULD);
            booleanQuery.Add(query2, Occur.SHOULD);

            var hits = indexSearcher.Search(booleanQuery, 20);
            if (!hits.ScoreDocs.Any())
            {
                Console.WriteLine("查無相關結果。");
                return;
            }
            Document doc;
            foreach (var hit in hits.ScoreDocs)
            {
                doc = indexSearcher.Doc(hit.Doc);
                Console.WriteLine("Score :" + hit.Score + ", Id :" + doc.Get("Id") + ", Name :" + doc.Get("Name") + ", Description :" + doc.Get("Description"));
            }
        }
    }
}

List<Product> GetProductsInformation()
{
    return new List<Product> {
            new Product{ Id = 1, Name = "蘋果", Description = "一天一蘋果，醫生遠離我。"},
            new Product{ Id = 2, Name = "橘子", Description = "橘子是醫生給娜美最珍貴的寶藏。"},
            new Product{ Id = 3, Name = "梨子", Description = "我是梨子，比蘋果好吃多囉!"},
            new Product{ Id = 4, Name = "葡萄", Description = "吃葡萄不吐葡萄皮，不吃葡萄倒吐葡萄皮"},
            new Product{ Id = 5, Name = "榴槤", Description = "水果界的珍寶!好吃一直吃。"}
        };
}

List<Product> GetUpdateProductsInformation()
{
    return new List<Product>
    {
        new Product{ Id = 6, Name = "香蕉", Description = "運動完後吃根香蕉補充養分。"},
        new Product{ Id = 2, Name = "橘子", Description = "橘子跟柳丁你分得出來嗎?"}
    };
}
class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}