
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Analysis.PanGu;

namespace PanGuLucene.Demo
{
    class Program
    {
        static DirectoryInfo INDEX_DIR = new DirectoryInfo("index");
        static Analyzer analyzer = new PanGuAnalyzer(); //MMSegAnalyzer //StandardAnalyzer

        static void Main(string[] args)
        {
            string[] texts = new string[] {
                "幾個星期來台灣主權歸屬問題再度成為熱門話題，盡忠職守的中華民國國史館林滿紅館長認為1952年在台北簽訂的《中日和約》有足夠條文說明台澎主權轉移給中華民國，而呂前副總統則認為日本在1951年舊金山和約已經向同盟國放棄台澎主權（中華民國因代表權問題未獲參加），因此1952年日本再也沒有台澎主權可以轉移給中華民國。台澎主權歸屬問題一直是統獨論戰的重要議題，現在又成為台灣傑出、哈佛出身的兩位女性領袖的論爭。林館長服務於中研院時，筆者有機會多次與他交談，得知他的論點。筆者雖然尊重他的專業，但是覺得日本早在1972年就已聲明終止該雙邊和約，現在世界上除了中華民國政府還緊抱著這條約不放之外，大概找不到其他國家關心它。因科技事務開會，有機會在總統府和台北賓館，公餘多次聽到呂前副總統暢談台澎主權歸屬，筆者很欣賞他深入問題，思考邏輯嚴謹，因而論點很具說服力，況且筆者也懷疑無條件投降的戰敗國日本，1952年時本身國家難保，哪還有權利代表同盟國轉移任何主權。呂強調1945年聯合國成立，既然聯合國憲章強調人民平等權利及自決原則，所以未定的台澎主權歸屬應該由台澎居民自決。台灣主權歸屬論戰其實侷限於台灣政治人物，沒有幾個外國人關心。筆者認為採用國際條約或國際法來決定主權歸屬的爭論，沒有多大意義，也不會有什麼效果，1972年日本片面終止《中日和約》不就是最好的例子。歷史上各國因權宜之計訂約與毀約，而且所謂的條約和國際法多半是按照超強大國(super powers)的利益訂定，新興國家的主權和領土的劃分還不都是這些國家為了保持勢力平衡的產物，聯合國憲章僅是遠程理想，自決也只是原則而已。就像東帝汶，人民要自決，台灣要主權，唯一途徑是長期激烈抗爭，以引起國際注意，最後由於列強擔心世界秩序被破壞，伸出援手解決主權爭議。",
                "好友的母親出門倒垃圾，一輛急駛摩托車猛然撞擊，就此倒地不起。這位伯母原本有心臟宿疾，家裡隨時準備著氧氣筒。然而萬萬沒有料到，她是用這種方式離開。子女完全不能接受，哭著說：「媽媽一句交代都沒就走了！」他們以為，媽媽即使心臟病發作，也總還有時間跟他們說說話，交代幾句，怎麼可以一聲不響就走呢？其實，他們忘了，媽媽每天都在交代。就跟天下的母親一樣，無非是「注意身體，小心著涼」、「不要太累，少熬夜，少喝酒」、「好好念書，別整天貪玩」......只不過我們聽得太多，聽得我們煩膩、麻木。直到母親閉口的那刻，我們才發現，還有很多話來不及聽、來不及問、來不及跟媽媽說。"
            };

            IndexWriter iw = new IndexWriter(FSDirectory.Open(INDEX_DIR), analyzer, true, IndexWriter.MaxFieldLength.LIMITED);
            int i = 0;
            foreach (string text in texts)
            {
                Document doc = new Document();
                doc.Add(new Field("body", text, Field.Store.YES, Field.Index.ANALYZED));
                iw.AddDocument(doc);
                Console.WriteLine("Indexed doc: {0}", text);
            }
            iw.Commit();
            iw.Optimize();
            iw.Dispose();

            //Analyzer a = new PanGuAnalyzer();
            //string s = "上海东方明珠";
            //System.IO.StringReader reader = new System.IO.StringReader(s);
            //Lucene.Net.Analysis.TokenStream ts = a.TokenStream(s, reader);
            //bool hasnext = ts.IncrementToken();
            //Lucene.Net.Analysis.Tokenattributes.ITermAttribute ita;
            //while (hasnext)
            //{
            //    ita = ts.GetAttribute<Lucene.Net.Analysis.Tokenattributes.ITermAttribute>();
            //    Console.WriteLine(ita.Term);
            //    hasnext = ts.IncrementToken();
            //}
            //ts.CloneAttributes();
            //reader.Close();
            //a.Close();
            //Console.ReadKey();



            Console.WriteLine();

            Console.WriteLine("Building index done!\r\n\r\n");

            while (true)
            {
                Console.Write("Enter the keyword: ");
                string keyword = Console.ReadLine();
                Search(keyword);
                Console.WriteLine();
            }

        }


        static void Search(string keyword)
        {
            IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(INDEX_DIR), true);
            QueryParser qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "body", analyzer);
            Query query = qp.Parse(keyword); //2008年底  
            Console.WriteLine("query> {0}", query);


            TopDocs tds = searcher.Search(query, 10);
            Console.WriteLine("TotalHits: " + tds.TotalHits);
            foreach (ScoreDoc sd in tds.ScoreDocs)
            {
                Console.WriteLine(sd.Score);
                Document doc = searcher.Doc(sd.Doc);
                Console.WriteLine(doc.Get("body"));
            }

            searcher.Dispose();
        }
    }
}