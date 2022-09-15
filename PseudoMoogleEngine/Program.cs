public class Moogle
{
    public static void Main() {

        System.Console.WriteLine("Leyendo los txt...");

        MyDirectory newDirectory = new MyDirectory(@"../Content");
        
        System.Console.WriteLine("Generando diccionarios...");
        
        // Array que contiene la posicionde cada documento en el File con cada palabra
        // y la frecuencia normalizada con que aparecen en el documento
        // A su vez este es un array con los vectores de los terminos indexados del i-esimo txt
        Dictionary<string,double>[] NumberOfTxtWithTF = MakeTFNormalizedForAll(newDirectory.directories);

        //IDF
        Dictionary<string,int> DocumentIDF = MakeIDF(NumberOfTxtWithTF);

        //The weigh of ith word in the jth documentprivate
        Dictionary<string,double>[] NumberTxtWithWordsWeighed = ToWeigh(NumberOfTxtWithTF, DocumentIDF);

        System.Console.WriteLine("Hola. Introduzca su busqueda");
        
        string query = Console.ReadLine();
        
        if(query == string.Empty) {
            System.Console.WriteLine("No sirvio");
        }
        else{
            //Query myquery = new Query(query);

        }


        //Vector de terminos indexados de la query
        /*
        string[] SplittedQuery = GiveAllSubsequencesStringAndLowerCased(query);

        Dictionary<string,double> QueryWeighed = MakeTFNormalized(SplittedQuery);
        QueryWeighed = ToWeighQuery(QueryWeighed, DocumentIDF);

        var Ranking = new List< KeyValuePair< double, int >>();

        Ranking = DoTheRanking(QueryWeighed,NumberTxtWithWordsWeighed);

        char[] FirstWordOfQuery = new char[SplittedQuery[0].Length];
        for(int i=0; i<query.Length; i++) {
            if(query[i] != ' ') FirstWordOfQuery[i] = query[i];
            else break;
        }

        string FirstkWordOfQuery = new string(FirstWordOfQuery);
        System.Console.WriteLine(FirstkWordOfQuery);

        foreach(KeyValuePair<double,int> kvp in Ranking) {
            if(kvp.Key != 0){
                GetTitle(Directories[kvp.Value]);
                GetSnippet(Directories[kvp.Value], FirstkWordOfQuery);
                System.Console.WriteLine(kvp.Key);
            }
        }
        */
    }

    
    
    
    
    // Aqui empiezan los metodos
    //. .........................................................
    //...........................................................
    //...........................................................
    
    
    
    

    public static string[] GiveAllSubsequencesStringAndLowerCased(string str) {
        // Primero dividimos las string en subcadenas formadas por cada palbra de la propia string
        // Implementar mejor cableado para los separadoes
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '!', '~', '`', '-', '(', ')', '#'};
        string[] subsequences = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        // Ahora lo convertimos todo a minusculas
        string[] Modified = new string[subsequences.Length];
        try {
            for(int i=0; i<Modified.Length; i++) {
                Modified[i] = subsequences[i].ToLower();
                Modified[i] = Modified[i].Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u').Replace('ñ','n');
                System.Console.WriteLine(Modified[i]);
            }
        }
        catch(Exception e) {
            System.Console.WriteLine(e.Message);
        }
        

        return Modified;
    }

    //Metodo para leer un archivo, guardarlo en un string y devolverlo
    static string[] ReadInTheDirectory(string directory) {
        string[] ReadText = File.ReadAllLines(directory);
        return ReadText;
    }    

    static void GetTitle(string Directory) {
        int indexContent = Directory.LastIndexOf('/');
        int indexTxt = Directory.LastIndexOf('.');

        string Title = new string(Directory.Substring(indexContent + 1,  indexTxt - indexContent - 1));

        System.Console.WriteLine(Title);
    }

    static void GetSnippet(string directory, string FirstkWordOfQuery) {
        string Text = File.ReadAllText(directory);
        int FirstOccurrence = Text.IndexOf(FirstkWordOfQuery);
        //System.Console.WriteLine("First Occurrence: " + FirstOccurrence);

        for(int i=FirstOccurrence;  i<Text.Length; i++) {
            System.Console.Write(Text[i]);
        }
    }

    public static Dictionary<string,double>[] MakeTFNormalizedForAll(string[] Directories){
        Dictionary<string,double>[] NumberOfTxtWithTF = new Dictionary <string, double> [Directories.Length];

        for(int i=0; i<Directories.Length; i++) {
            NumberOfTxtWithTF[i] = new Dictionary<string, double>();
            // Implementarlo con ReadAllText
            
            string[] Text = ReadInTheDirectory(Directories[i]);
            
            for(int j=0; j<Text.Length; j++){
                string[] TextSplitted = GiveAllSubsequencesStringAndLowerCased(Text[j]);
                
                Dictionary<string,double> NewsPairToAdd = MakeTFNormalized(TextSplitted);  
                
                foreach(KeyValuePair < string,double > kvp in NewsPairToAdd) {
                    if(!NumberOfTxtWithTF[i].ContainsKey(kvp.Key)) {
                        NumberOfTxtWithTF[i].Add(kvp.Key, 1);
                    }
                    else {
                        NumberOfTxtWithTF[i][kvp.Key]++;
                    }
                }
            }
        }  


        // Hallamos su frecuencia relativa(normalizar)
        for(int i=0; i<NumberOfTxtWithTF.Length; i++) {
            foreach(KeyValuePair<string,double> kvp in NumberOfTxtWithTF[i]) {
                NumberOfTxtWithTF[i][kvp.Key] = kvp.Value/ NumberOfTxtWithTF[i].Count;
            }
        }

        return NumberOfTxtWithTF;
    }

    static Dictionary<string,double> MakeTFNormalized(string[] Splitted){
        Dictionary<string, double> TermFrequency = new Dictionary<string, double>();
        
        for(int i=0; i<Splitted.Length; i++) {
            if(!TermFrequency.ContainsKey(Splitted[i])) {
                TermFrequency.Add(Splitted[i], 1);
            }

            else {
                TermFrequency[Splitted[i]]++;
            }
        }

        return TermFrequency;
    }

    static Dictionary<string,int> MakeIDF(Dictionary<string,double>[] NumberOfTxtWithTF){
        Dictionary<string,int> DocumentIDF = new Dictionary<string, int>();
        
        for(int i=0; i<NumberOfTxtWithTF.Length; i++) {
            foreach(KeyValuePair < string,double > kvp in NumberOfTxtWithTF[i]) {
                if(!DocumentIDF.ContainsKey(kvp.Key)) {
                    DocumentIDF.Add(kvp.Key, 1);
                }
                else {
                    DocumentIDF[kvp.Key]++;
                }
            }
        }

        return DocumentIDF;   
    }

    public static Dictionary<string, double>[] ToWeigh(Dictionary<string, double>[] NumberOfTxtWithTF,
    Dictionary<string, int> DocumentIDF)
    {
        Dictionary<string,double>[] NumberTxtWithWordsWeighed = NumberOfTxtWithTF;

        for(int i=0; i<NumberOfTxtWithTF.Length; i++) {
            foreach(KeyValuePair <string,double> kvp in NumberOfTxtWithTF[i]) {
                NumberTxtWithWordsWeighed[i][kvp.Key] *= (DocumentIDF[kvp.Key])*1.0;
            }
        }

        return NumberTxtWithWordsWeighed;;
    }

    static Dictionary<string,double> ToWeighQuery(Dictionary<string,double> ProcessedQuery,
    Dictionary<string,int> DocumentIDF) {
            Dictionary<string,double> QueryWeighed = new Dictionary<string, double>();

            foreach(KeyValuePair <string,double> kvp in ProcessedQuery) {
                if(!DocumentIDF.ContainsKey(kvp.Key)) {
                   QueryWeighed.Add(kvp.Key, 0);
                }

                else {
                    QueryWeighed.Add(kvp.Key, CalculateWeigh(kvp.Value, DocumentIDF[kvp.Key] * 1.0, DocumentIDF.Count * 1.0));
                }
                if(QueryWeighed[kvp.Key] == 0) QueryWeighed.Remove(kvp.Key);
            }

            return QueryWeighed;
    }
    static double CalculateWeigh(double freq, double idf, double total) {
        double a = 0.4;

        return (a + (1 - a) * freq) * Math.Log(total/idf, 10);
    }

    static List <KeyValuePair<double,int>> DoTheRanking(Dictionary<string,double> QueryWeighed, Dictionary<string,double>[] NumberTxtWithWordsWeighed) {
        var RankingList = new List< KeyValuePair<double,int>>();


        for(int i=0; i<NumberTxtWithWordsWeighed.Length; i++) {
            double dxq = 0;
            double modD = 0;
            double modQ = 0;
            foreach(KeyValuePair<string, double> kvp in QueryWeighed) {
                if(NumberTxtWithWordsWeighed[i].ContainsKey(kvp.Key)) {
                    dxq += (kvp.Value * NumberTxtWithWordsWeighed[i][kvp.Key]);
                    modD += (NumberTxtWithWordsWeighed[i][kvp.Key] * NumberTxtWithWordsWeighed[i][kvp.Key]);
                }
              
                modQ += (kvp.Value * kvp.Value);
            }

            double sim = 0;
            if(modD != 0) {
                sim += (dxq / (Math.Sqrt(modD) * Math.Sqrt(modQ)));
            }

            RankingList.Add(new KeyValuePair<double,int> (sim,i));
        }

        RankingList.Sort((x,y) => x.Key.CompareTo(y.Key));
        RankingList.Reverse();

        return RankingList;
    }
}


public class Query
{
    public string query;
    private string[] SplittedQuery;
    private Dictionary<string,double> queryTF;
    public Dictionary<string,double> queryTFIDF;

    public Query(string query, Dictionary<string,int> IDF) {
        this.query = query;
        this.SplittedQuery = SplitModified(this.query);
        this.queryTF = MakeQueryTF(this.SplittedQuery);
        this.queryTFIDF = MakeQueryTFxIDF(this.queryTF, IDF);
    }   

    static private string[] SplitModified(string query) {        
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '~', '`', '-', '(', ')','#',
        '@', '$', '%', '^', '&', '*', '_', '+', '=', '<', '>'};
        string[] subsequences = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        string[] Modified = new string[subsequences.Length];
        for(int i=0; i<Modified.Length; i++) {
            Modified[i] = subsequences[i].ToLower();
            Modified[i] = Modified[i].Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u').Replace('ñ','n');
        }

        return Modified;
    }

    static private Dictionary<string,double> MakeQueryTF(string[] Words){
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

        foreach(KeyValuePair <string, double> kvp in TermFrequency) {
            TermFrequency[kvp.Key] = kvp.Value / MaxTF;
        }

        return TermFrequency;
    }

    static private Dictionary<string, double> MakeQueryTFxIDF(Dictionary<string, double> queryTF,
    Dictionary<string,int> IDF) {
        var queryTFIDF = new Dictionary<string, double>();

        foreach(KeyValuePair <string, double > kvp in queryTF) {
            if(IDF.ContainsKey(kvp.Key)) {
                queryTFIDF.Add(kvp.Key, CalculateTFxIDF(kvp.Value, IDF[kvp.Key] * 1.0, IDF.Count * 1.0));
            }
            else {
                queryTFIDF.Add(kvp.Key, CalculateTFxIDF(kvp.Value, 0, IDF.Count));
            }
        }

        return queryTFIDF;
    }
    
    static private double CalculateTFxIDF(double freq, double idf, double total) {
        double a = 0.4;

        return (a + (1 - a) * freq) * Math.Log(total/idf, 10);
    }
}

public class MyDirectory
{
    public string path;
    public string[] directories;

    public MyDirectory (string path){
        this.path = path;
        this.directories = ProcessDirectory(this.path);
    }
    private static string[] ProcessDirectory(string targetDirectory)
    {
        // Process the list of files found in the directory.
        string [] fileEntries = Directory.GetFiles(targetDirectory);
        return fileEntries;
    }
}


// Por desarrollar mas la clase Document
public class Document 
{
    public string DocDirectory;
    public string Title;
    private string Text;
    private string[] SplittedText;
    public Dictionary<string,double> TF;

    public Document(string Directory) {
        this.DocDirectory = Directory;
        this.Title = GetTitle(Directory);
        this.Text = ReadText(Directory);
        this.SplittedText = SplitModified(this.Text);
        this.TF = MakeTF(this.SplittedText);
    }

    static private string GetTitle(string Directory) {
        int indexContent = Directory.LastIndexOf('/');
        int indexTxt = Directory.LastIndexOf('.');

        string Title = new string(Directory.Substring(indexContent + 1,  indexTxt - indexContent - 1));

        return Title;
    }

    static private string ReadText(string Directory) {
        string ReadText = File.ReadAllText(Directory);
        return ReadText;
    }  

    static private string[] SplitModified(string Text) {        
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '~', '`', '-', '(', ')','#',
        '@', '$', '%', '^', '&', '*', '_', '+', '=', '<', '>'};
        string[] subsequences = Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        string[] Modified = new string[subsequences.Length];
        for(int i=0; i<Modified.Length; i++) {
            Modified[i] = subsequences[i].ToLower();
            Modified[i] = Modified[i].Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u').Replace('ñ','n');
        }

        return Modified;
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

        foreach(KeyValuePair <string, double> kvp in TermFrequency) {
            TermFrequency[kvp.Key] = kvp.Value / MaxTF;
        }

        return TermFrequency;
    }
}

public class AllDocuments
{
    string[] directories;
    Document[] mydocuments;    
    Dictionary<string, int> IDF;

    public AllDocuments(string[] directories) {
        this.directories = directories;
        
        this.mydocuments = new Document[directories.Length];
        for(int i=0; i<directories.Length; i++) {
            this.mydocuments[i] = new Document(directories[i]);
        }

        this.IDF = MakeIDF(this.mydocuments);
    }

    static public Dictionary<string,int> MakeIDF(Document[] mydocuments) {
        var DocumentIDF = new Dictionary<string, int>();
        
        for(int i=0; i<mydocuments.Length; i++) {
            foreach(KeyValuePair < string,double > kvp in mydocuments[i].TF) {
                if(!DocumentIDF.ContainsKey(kvp.Key)) {
                    DocumentIDF.Add(kvp.Key, 1);
                }
                else {
                    DocumentIDF[kvp.Key]++;
                }
            }
        }

        return DocumentIDF;
    }

}