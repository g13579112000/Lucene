// See https://aka.ms/new-console-template for more information
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;

public static class IndexFiles
{
    internal static readonly DirectoryInfo INDEX_DIR = new DirectoryInfo("index");

    [STAThread]
    public static void Main(string[] args)
    {
        var txtPath = "D:\\Test\\Lucene\\LuceneTest.Txt";
        if( File.Exists(INDEX_DIR.FullName) || System.IO.Directory.Exists(INDEX_DIR.FullName))
        {
            Console.Out.WriteLine("Cannot save index to '" + INDEX_DIR + "' directory, please delete it first");
            Environment.Exit(1);
        }

        var docDir = new DirectoryInfo(txtPath);
        var docDirExists = File.Exists(docDir.FullName) || System.IO.Directory.Exists(docDir.FullName);
        if (!docDirExists)
        {
            Console.Out.WriteLine("Document directory '" + docDir.FullName + "' does not exist or is not readable, please check the path");
            Environment.Exit(1);
        }

        var start = DateTime.Now;
        try
        {
            using(var writer = new IndexWriter(FSDirectory.Open(docDir), new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED))
            {
                Console.Out.WriteLine("Indexing to directory '" + docDir + "'...");
                IndexDirectory(writer, docDir);
                Console.Out.WriteLine("Optimizing...");
                writer.Optimize();
                writer.Commit();
            }
            var end = DateTime.Now;
            Console.Out.WriteLine("total millisecond: " + (end.Millisecond - start.Millisecond));
        }
        catch(IOException ex)
        {
            Console.Out.WriteLine(" caught a " + ex.GetType() +"\n with message: " + ex.Message);
        }
    }

    internal static void IndexDirectory(IndexWriter writer, DirectoryInfo directory)
    {
        foreach(var subDirectory in directory.GetDirectories())
        {
            IndexDirectory(writer, subDirectory);
        }
        foreach(var file in directory.GetFiles())
        {
            IndexDocs(writer, file);
        }
    }

    internal static void IndexDocs(IndexWriter writer, FileInfo file)
    {
        Console.Out.WriteLine("adding " + file);
        try
        {
            writer.AddDocument(FileDocument.Document(file));
        }
        catch (Exception)
        {

        }
    }
}

public static class FileDocument
{
    public static Document Document(FileInfo file)
    {
        Document doc = new Document();
        doc.Add(new Field("path", file.FullName, Field.Store.YES, Field.Index.NOT_ANALYZED));
        doc.Add(new Field("modified", DateTools.TimeToString(file.LastWriteTime.Millisecond, DateTools.Resolution.MINUTE), Field.Store.YES, Field.Index.NOT_ANALYZED));
        doc.Add(new Field("contents", new StreamReader(file.FullName, System.Text.Encoding.Default)));
        return doc;
    }
}