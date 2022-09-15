namespace MoogleEngine;

public class MyDirectory
{
    // Lee la carpeta y almacena la direccion de cada txt

    public string path;
    private string[] directories;

    public MyDirectory (){
        this.path = "@../Content";
        this.directories = Directory.GetFiles(this.path);
    }

    public void GetDirectories() {
        foreach(string dir in directories) {
            System.Console.WriteLine(dir);
        }
    }
}

public class Document 
{
    public string DocDirectory;
    public string Text;
    public string Title;
    public string[] SplittedText;
    private string[] SplittedTextModified;
    public Dictionary <string,string> OriginalWithNormalized;
    public Dictionary<string,double> TF;

    public Document(string Directory) {
        this.DocDirectory = Directory;
        this.Text = ReadText(Directory);
        this.Title = GetTitle(Directory);
        this.SplittedText = FirstSplit(this.Text);     
        this.SplittedTextModified = Modified(SplittedText);   
        this.OriginalWithNormalized = MatchOrigStringWithNormString(this.SplittedText,
        this.SplittedTextModified);
        this.TF = MakeTF(this.SplittedTextModified);
    }

    static public string GetTitle(string Directory) {
        int indexContent = Directory.LastIndexOf('/');
        int indexTxt = Directory.LastIndexOf('.');

        string Title = new string(Directory.Substring(indexContent + 1,  indexTxt - indexContent - 1));

        return Title;
    }

    static private string ReadText(string Directory) {
        string ReadText = File.ReadAllText(Directory);
        return ReadText;
    }  

    private string[] FirstSplit(string Text) {        
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '~', '`', '-', '(', ')','#','@',
        '"','$', '%', '^', '&', '*', '_', '+', '=', '<', '>','0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        string[] subsequences = Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        return subsequences;
    }

    private string[] Modified(string[] SplittedText) {
        string[] modified = new string[SplittedText.Length];
        for(int i=0; i<modified.Length; i++) {
            modified[i] = SplittedText[i].ToLower();
            //modified[i] = NormalizeDiacritics(modified[i]);
        }   

        return modified;
    }

    static private Dictionary<string,double> MakeTF(string[] Words){
        Dictionary<string, double> TermFrequency = new Dictionary<string, double>();
        
        for(int i=0; i<Words.Length; i++) {
            if(!TermFrequency.ContainsKey(Words[i])) {
                TermFrequency.Add(Words[i], 1);
            }
            else {
                TermFrequency[Words[i]]++;
            }
        }

        double MaxTF = 0;
        foreach(KeyValuePair <string, double> kvp in TermFrequency) {
            MaxTF = Math.Max(MaxTF, kvp.Value);
        }

        foreach(KeyValuePair <string, double> kvp in TermFrequency){
            TermFrequency[kvp.Key] = (kvp.Value/MaxTF);
        }

        return TermFrequency;
    }

    public Dictionary<string,string> MatchOrigStringWithNormString(string[] FirstSplit, string[]
    ModifiedSplit){
        
        var MatchedString = new Dictionary<string,string>();
        for(int i = 0; i<ModifiedSplit.Length; i++) {
            if(!MatchedString.ContainsKey(ModifiedSplit[i])){
                MatchedString.Add(ModifiedSplit[i], FirstSplit[i]);
            }
        }

        return MatchedString;
    }

    public void GetWord(string word) {
        System.Console.WriteLine(this.OriginalWithNormalized[word]);
    }
}

public class AllDocuments
{
    string[] directories;
    public Document[] TheDocuments;    
    public Dictionary<string, double> IDF;
    public Dictionary<string, double>[] TFIDF;

    public AllDocuments(string[] directories) {
        this.directories = directories;
        
        this.TheDocuments = new Document[this.directories.Length];
        for(int i=0; i<directories.Length; i++) {
            this.TheDocuments[i] = new Document(directories[i]);
        }

        
        this.IDF = MakeIDF(this.TheDocuments);
        this.TFIDF = MakeTFIDF(this.TheDocuments, this.IDF);
    }

    static public Dictionary<string,double> MakeIDF(Document[] TheDocuments) {
        var IDF = new Dictionary<string, double>();
        
        for(int i=0; i<TheDocuments.Length; i++) {
            foreach(KeyValuePair < string,double > kvp in TheDocuments[i].TF) {
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

    public static Dictionary<string, double>[] MakeTFIDF(Document[] TheDocuments,
    Dictionary<string, double> IDF)
    {
        var TFIDF = new Dictionary<string, double>[TheDocuments.Length];

        // Paso por cada documento leyendo 
        for(int i=0; i<TheDocuments.Length; i++) {
            // Creo un diccionario para el documento i-esimo donde guardo su TF-IDF
            var temp = new Dictionary<string,double>();
            foreach(KeyValuePair <string,double> kvp in TheDocuments[i].TF) {
                temp.Add(kvp.Key,  IDF[kvp.Key]* TheDocuments[i].TF[kvp.Key]);
            }
            TFIDF[i] = temp;
        }

        return TFIDF;
    }

    public List<string> GetSnippet(Dictionary<string,double> qTFIDF, int posDocument) {
        // Busquemos la palabra con mayor peso en la query que le haremos corresponder al snippet

        string MIWOQ = "No hay"; // Most Important word for query
        double Importance = 0;
        foreach(KeyValuePair < string, double> kvp in qTFIDF) {
            if(this.TFIDF[posDocument].ContainsKey(kvp.Key)) {
                if(Importance < kvp.Value) {
                    MIWOQ = kvp.Key;
                    Importance = kvp.Value;
                }
            }
        }

        var snippet = new List<string> ();

        // Buscamos la posicion de la palabra mas importante para la query en su texto

        int posMIWOQ = 0;
        for(int j = 0; j<this.TheDocuments[posDocument].SplittedText.Length; j++) {
            if(this.TheDocuments[posDocument].SplittedText[j] == MIWOQ) {
                posMIWOQ = j;
                break;
            }
        }

        System.Console.WriteLine();
        System.Console.WriteLine(MIWOQ + " posicion " + posMIWOQ);
        System.Console.WriteLine();

        
        // Aqui decidimos el max numero de palabras que tendra el snippet con centro en la MIWOF
        var SnippetRange = RangeOfSnippet(20 , posMIWOQ, posMIWOQ, 
        this.TheDocuments[posDocument].SplittedText.Length);

        for(int j = SnippetRange.first; j<=SnippetRange.second; j++) {
            snippet.Add(this.TheDocuments[posDocument].SplittedText[j]);
        }


        return snippet;
    }
    Pair RangeOfSnippet(int CountOfWords, int left, int right, int len) {
        if(CountOfWords == 0) return new Pair(left, right);
        if(left - 1 >= 0 && right + 1 < len) {
            return RangeOfSnippet(CountOfWords-=2, left -1 , right + 1, len);
        }
        if(left - 1 >= 0 && right + 1 >= len) {
            return RangeOfSnippet(CountOfWords--, left - 1, right, len);
        }
        if(left - 1 < 0 && right + 1 < len) {
            return RangeOfSnippet(CountOfWords--, left, right + 1, len);
        }

        return new Pair(left, right);
    }
}

public partial class Pair
{
    public Pair(int first, int second) 
    {
        this.first = first;
        this.second = second;
    }
}

public partial class Pair
{
    public int first {get;set;}
    public int second {get;set;}

}