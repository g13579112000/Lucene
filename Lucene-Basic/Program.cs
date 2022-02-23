using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.DependencyInjection;


var serviceCollection = new ServiceCollection();

serviceCollection.AddTransient<App>();
serviceCollection.AddTransient<IService, EnService>(); //切換Repository語系 ChService or EnService

var serviceProvider = serviceCollection.BuildServiceProvider();

serviceProvider.GetRequiredService<App>().Run();

class App
{
    private readonly IService _service;
    private readonly DirectoryInfo _dir;

    public App(IService service)
    {
        _service = service;
        _dir = new DirectoryInfo("LuceneDocument");
    }

    public void Run()
    {
        // 取得或建立Lucene文件資料夾
        if (!File.Exists(_dir.FullName))
        {
            System.IO.Directory.CreateDirectory(_dir.FullName);
        }

        // Asp.Net Core需要於Nuget安裝System.Configuration.ConfigurationManager提供用戶端應用程式的組態檔存取
        Lucene.Net.Store.Directory directory = FSDirectory.Open(_dir);

        //選擇分詞器
        var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);

        //使用RAM提高效能
        //var directory = new RAMDirectory(dir);

        CreateIndex(_service.GetProductsInformation(), directory, analyzer);

        // 搜尋關鍵字取得結果
        while (true)
        {
            Console.Write("請輸入欲查詢字串 :");
            var searchValue = Console.ReadLine();
            Search(searchValue, directory, analyzer);
        }

    }


    void CreateIndex(List<Product> information, Lucene.Net.Store.Directory directory, StandardAnalyzer analyzer)
    {
        //var repository = new Repository();
        var indexWriter = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);

        //var information = repository.GetProductsInformation();

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
        indexWriter.Dispose();
    }

    void Search(string searchValue, Lucene.Net.Store.Directory directory, StandardAnalyzer analyzer)
    {

        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, "Description", analyzer).Parse(searchValue);
        var indexSearcher = new IndexSearcher(directory);

        // 搜尋取數
        var queryLimit = 20;

        var hits = indexSearcher.Search(parser, queryLimit);

        if (!hits.ScoreDocs.Any())
        {
            Console.WriteLine("查無相關結果。");
            return;
        }

        foreach (var hit in hits.ScoreDocs)
        {
            Document doc = indexSearcher.Doc(hit.Doc);
            Console.WriteLine("Score :" + hit.Score + ", Id :" + doc.Get("Id") + ", Name :" + doc.Get("Name") + ", Description :" + doc.Get("Description"));
        }
    }
}


interface IService
{
    List<Product> GetProductsInformation();
}

class ChService : IService
{
    public List<Product> GetProductsInformation()
    {
        return new List<Product> {
            new Product{ Id = 1, Name = "蘋果", Description = "一天一蘋果，醫生遠離我。"},
            new Product{ Id = 2, Name = "橘子", Description = "醫生給娜美最珍貴的寶藏。"},
            new Product{ Id = 3, Name = "梨子", Description = "我是梨子，比蘋果好吃多囉!"},
            new Product{ Id = 4, Name = "葡萄", Description = "吃葡萄不吐葡萄皮，不吃葡萄倒吐葡萄皮"},
            new Product{ Id = 5, Name = "榴槤", Description = "水果界的珍寶!好吃一直吃。"}
        };
    }
}

class EnService : IService
{
    public List<Product> GetProductsInformation()
    {
        return new List<Product> {
            new Product{ Id = 1, Name = "Apple", Description = "An apple a day keeps the doctor away."},
            new Product{ Id = 2, Name = "Orange", Description = "The most precious treasure the doctor gave to Nami."},
            new Product{ Id = 3, Name = "Pear", Description = "I am Pear, which is much better than an apple!"},
            new Product{ Id = 4, Name = "Grape", Description = "Eating grapes but not spitting the skin, spitting the grape skin without eating grapes."},
            new Product{ Id = 5, Name = "Durian", Description = "Treasures in the fruit world! Delicious and always eating."}
        };
    }
}

class Product
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}