namespace MoogleEngine;

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

public partial class Document 
{
    private string DocDirectory;
    private string Text;
    private string[] Tokens;
    private Dictionary<string,double> TF;

    public Document(string Directory) {
        this.DocDirectory = Directory;
        this.Text = ReadText(Directory);

        this.Tokens = NormalizeText(this.Text);  
        // CalculaAveragePositionOfEachWord(); 
        this.TF = MakeTF(this.Tokens);
    }

    public string GetText() {
        return Text;
    }

    public string GetTitle() {
        int indexContent = DocDirectory.LastIndexOf('/');
        int indexTxt = DocDirectory.LastIndexOf('.');

        string Title = new string(DocDirectory.Substring(indexContent + 1,  indexTxt - indexContent - 1));

        return Title;
    }

    private string ReadText(string Directory) {
        string ReadText = File.ReadAllText(Directory);
        return ReadText;
    }  

    public string[] GetTokens() {
        return Tokens;
    }

    public Dictionary<string,double> GetTF() {
        return TF;
    }
}


// Metodos mas complejos
public partial class Document
{
    private string[] NormalizeText(string Text) {       

        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '~', '`', '-', '(', ')',
        '#','@', '"','$', '%', '^', '&', '*', '_', '+', '=', '<', '>'};
        string[] subsequences = Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        for(int i=0; i<subsequences.Length; i++) {
            subsequences[i] = subsequences[i].ToLower();
            
            subsequences[i] = subsequences[i].Replace('á', 'a');
            subsequences[i] = subsequences[i].Replace('é', 'e');
            subsequences[i] = subsequences[i].Replace('í', 'i');
            subsequences[i] = subsequences[i].Replace('ó', 'o');
            subsequences[i] = subsequences[i].Replace('ú', 'u'); 
        }   

        return subsequences;
    }

    private Dictionary<string,double> MakeTF(string[] Words){
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

    public Dictionary<string,double> CreateVectorDoc(Dictionary<string,double> qTFIDF,
                                                     Dictionary<string,double> IDF) {

        var VectorDoc = new Dictionary<string,double>();

        foreach(var kvp in qTFIDF) {
            if(TF.ContainsKey(kvp.Key)) {
                VectorDoc.Add(kvp.Key, TF[kvp.Key]*IDF[kvp.Key]);
            }
            else {
                VectorDoc.Add(kvp.Key, 0);
            }
        }

        return VectorDoc;
    }
}

public class AllDocuments
{
    private string[] directories;
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


