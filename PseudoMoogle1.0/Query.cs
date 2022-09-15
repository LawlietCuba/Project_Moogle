namespace MyMoogleEngine;

public class Query
{
    public string query;
    private string[] SplittedQuery;
    public Dictionary<string,double> qTFIDF;
    public List<KeyValuePair< double,int >> RankingList;

    public Query(string query, AllDocuments Doc) {
        this.query = query;
        this.SplittedQuery = SplitModified(this.query);
        this.qTFIDF = MakeQueryTFxIDF(this.SplittedQuery, Doc.IDF);
        this.RankingList = DoTheRanking(this.qTFIDF, Doc);
    }   

    static private string[] SplitModified(string query) { 
        // Normalizar la query       
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '`', '-', '(', ')','#',
        '@', '$', '%', '^', '&', '_', '+', '=', '<', '>'};
        string[] subsequences = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        string[] Modified = new string[subsequences.Length];
        for(int i=0; i<Modified.Length; i++) {
            Modified[i] = subsequences[i].ToLower();

        }

        return Modified;
    }

    // Hace el TF-IDF de la query
    static private Dictionary<string,double> MakeQueryTFxIDF(string[] Words,
    Dictionary<string, double> IDF){
        var TermFrequency = new Dictionary<string, double>();
        
        // Hace un diccionario con un conteo simple de las palabras
        for(int i=0; i<Words.Length; i++) {
            if(!TermFrequency.ContainsKey(Words[i])) {
                TermFrequency.Add(Words[i], 1);
            }
            else {
                TermFrequency[Words[i]]++;
            }
        }

        // Busco la palabra con el numero mayor de repeticiones para sacar la frecuencia de cada una y
        // la palabra que tenga valor 1 es la mas importante de la query
        double MaxTF = 0;
        foreach(KeyValuePair <string, double> kvp in TermFrequency) {
            MaxTF = Math.Max(MaxTF, kvp.Value);
        }

        foreach(KeyValuePair <string, double> kvp in TermFrequency) {
            TermFrequency[kvp.Key] = kvp.Value / MaxTF;
        }

        // Utilizando el TF calculado y el IDF de los documentos hacer el TFIDF en la query
        var qTFIDF = new Dictionary<string, double>();

        foreach(KeyValuePair <string, double > kvp in TermFrequency) {
            if(IDF.ContainsKey(kvp.Key)) {
                qTFIDF.Add(kvp.Key, CalculateTFxIDF(kvp.Value, IDF[kvp.Key]));
            }
            else {
                qTFIDF.Add(kvp.Key, CalculateTFxIDF(kvp.Value, 0));
            }
        }

        return qTFIDF;
    }
    
    static private double CalculateTFxIDF(double freq, double idf) {
        double a = 0.4;

        return (a + (1 - a) * freq * idf);
    }

    static private List <KeyValuePair<double,int>> DoTheRanking(Dictionary<string,double> qTFIDF,
    AllDocuments Do) {
        var RankingList = new List< KeyValuePair<double,int> >();  

        for(int i=0; i<Do.TFIDF.Length; i++) {
            double DxQ = 0;
            double modD = 0;
            double modQ = 0;
            foreach(KeyValuePair<string, double> kvp in qTFIDF) {
                if(Do.TFIDF[i].ContainsKey(kvp.Key)) {
                    DxQ += (kvp.Value * Do.TFIDF[i][kvp.Key]);
                    modD += (Do.TFIDF[i][kvp.Key] * Do.TFIDF[i][kvp.Key]);
                }
              
                modQ += (kvp.Value * kvp.Value);
            }

            double sim = 0;
            if(modD != 0) {
                sim = (DxQ / (Math.Sqrt(modD) * Math.Sqrt(modQ)));
            }


            RankingList.Add(new KeyValuePair<double,int> (sim,i));
        }

        RankingList.Sort((x,y) => x.Key.CompareTo(y.Key));
        RankingList.Reverse();

        return RankingList;
    }

   
}