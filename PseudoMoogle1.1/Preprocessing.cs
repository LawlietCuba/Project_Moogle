namespace MyMoogleEngine;

public class MyDirectory
{
    // Lee la carpeta y almaacena la direccion de cada txt

    private string path;
    private string[] directories;

    public MyDirectory (){
        this.path = @"../Content";
        this.directories = Directory.GetFiles(path);
    }

    public string[] GetDirectories() {
        return directories;
    }
}
public class AllDocuments
{
    string[] directories;
    public Document[] TheDocuments;    
    public Dictionary<string, double> IDF;

    public AllDocuments(string[] directories) {
        this.directories = directories;
        
        this.TheDocuments = new Document[this.directories.Length];
        
        for(int i=0; i<directories.Length; i++) {
            this.TheDocuments[i] = new Document(directories[i]);
        }

        this.IDF = MakeIDF(this.TheDocuments);
    }

    static public Dictionary<string,double> MakeIDF(Document[] TheDocuments) {
        var IDF = new Dictionary<string, double>();
        
        for(int i=0; i<TheDocuments.Length; i++) {
            foreach(KeyValuePair < string,double > kvp in TheDocuments[i].GetTF()) {
                if(!IDF.ContainsKey(kvp.Key)) {
                    IDF.Add(kvp.Key, 1);
                }
                else {
                    IDF[kvp.Key]++;
                }
            }

        }

        double total = IDF.Count;
        foreach(KeyValuePair <string, double> kvp in IDF) {
            IDF[kvp.Key] = Math.Log(total/kvp.Value, 10);
        }

        return IDF;
    }
}
public class Pair
{
    public int first {get;set;}
    public int second {get;set;}

    public Pair(int first, int second) 
    {
        this.first = first;
        this.second = second;
    }

    public Pair Sum(int a, int b) {
        return new Pair(first + a, second + b);
    }
}
