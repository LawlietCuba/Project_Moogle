namespace MyMoogleEngine;
public class Query
{
    private string query;
    private string[] TokensQ;
    private Dictionary<string,double> qTFIDF;
    private List<KeyValuePair< double,int >> RankingList;
    private int HowManyResults;                                 
    private string Suggestion;
    private List<string> OperExcla;                    // Lista de string con !
    private List<string> OperPow;                      // Lista de string con ^
    private Dictionary<string, string> OperClosen;     //Lista de string con ~ (cercania)
    private Dictionary<string, int> OperAster;         // Lista de pares de string con *
    public Query(string query, AllDocuments Docs) {
        // Instanciamos cada una de las variables de los operadores
        this.OperExcla = new List<string>();
        this.OperPow = new List<string>();
        this.OperClosen = new Dictionary<string, string>();
        this.OperAster = new Dictionary<string, int>();
      

        this.query = query;
        this.TokensQ = ProcessQuery(this.query);
        FindTheOperators(TokensQ);

        this.qTFIDF = MakeQueryTFxIDF(this.TokensQ, Docs.IDF);

        // Pasemos por cada lista de operadores
        // Relevancia en orden ascendente de los operadores sera:
        // 1. Asteriscos
        // 2. Cercania
        // 3. Exclamacion
        // PD : Potencia solo nos afectara en la cantidad de documentos que devolvemos al usuario

        AppliAster(qTFIDF);

        this.HowManyResults = 0;
        this.RankingList = new List<KeyValuePair<double, int>>();
        Search(Docs);
        this.Suggestion = GiveASuggestion(Docs);
    } 

    public string GetSuggestion() {
        return Suggestion;
    }  

    public List< KeyValuePair <double,int> > GetRankingList() {
        return RankingList;
    }

    public int GetHowManyResults() {
        return HowManyResults;
    }

    public List<string> GetOperExcla() {
        return OperExcla;
    }
    public List<string> GetOperPow() {
        return OperPow;
    }
    public Dictionary<string, string> GetOperClosen() {
        return OperClosen;
    }
    public Dictionary<string, int> GetOperAster() {
        return OperAster;
    }

    public Dictionary<string,double> GetQTFIDF() {
        return qTFIDF;
    }
    static private string[] ProcessQuery(string query) { 
        
        // Normalizar la query       
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '`', '-', '(', ')','#',
        '@', '$', '%', '&', '_', '+', '=', '<', '>'};
        string[] subsequences = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        string[] Modified = new string[subsequences.Length];
        for(int i=0; i<Modified.Length; i++) {
            Modified[i] = subsequences[i].ToLower();
        }
        
        // Busquemos los operadores en la query  

        return Modified;
    }

    private void FindTheOperators(string[] WordsTokenized) {
        // PENDIENTE: Leer bien y la palabra cuando tenga dos operadores distintos
        // ------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------

        for(int i=0; i<WordsTokenized.Length; i++) {
            for(int j=0; j<WordsTokenized[i].Length; j++) {
                if(WordsTokenized[i][j] == '!' && !OperExcla.Contains(SaveThatWord(WordsTokenized[i], '!'))) {
                    OperExcla.Add(SaveThatWord(WordsTokenized[i], '!'));
                }
                if(WordsTokenized[i][j] == '^' && !OperPow.Contains(SaveThatWord(WordsTokenized[i], '^'))) {
                    OperPow.Add(SaveThatWord(WordsTokenized[i], '^'));
                }
                if(WordsTokenized[i][j] == '*' && !OperAster.ContainsKey(SaveThatWord(WordsTokenized[i], '*'))) {
                    OperAster.Add(SaveThatWord(WordsTokenized[i], '*'), CountAster(WordsTokenized[i], '*'));
                }
                if(WordsTokenized[i][j] == '~' && i-1 > 0 && i+1 <WordsTokenized[i].Length && !OperClosen.ContainsKey(SaveThatWord(WordsTokenized[i-1], ' '))) {
                    OperClosen.Add(SaveThatWord(WordsTokenized[i-1], ' '), SaveThatWord(WordsTokenized[i+1], ' '));
                }
            }
        }
    }
    private static string SaveThatWord(string str, char target) {
        string word = "";
        for(int j = 0; j < str.Length; j++) {
            if(str[j] != target) {
                word += str[j];
            }
        }

        return word;
    }

    private static int CountAster(string str, char target) {
        int count = 0;
        for(int j = 0; j<str.Length; j++) {
            if(str[j] == target) {

            }
        }

        return count;
    }
    
    // Hacer el TF-IDF de la query
    private Dictionary<string,double> MakeQueryTFxIDF(string[] Words,
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

        // Busco la palabra con el numero mayor de repeticiones para sacar la frecuencia de 
        //cada una y la palabra que tenga valor 1 es la mas importante de la query
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
    
    private double CalculateTFxIDF(double freq, double idf) {
        double a = 0.4;

        return (a + (1 - a) * freq * idf);
    }

    private void  Search(AllDocuments Docs) {
        for(int j = 0; j<Docs.TheDocuments.Length; j++){
            // Creamos el vector correspondiente a la query de cada documento
            var CorresVector = Docs.TheDocuments[j].CreateVectorDoc(qTFIDF, Docs.IDF);
            // Calculamos el score
            var Score = CalculateScore(CorresVector, qTFIDF);
            if(Score != 0){
                RankingList.Add(new KeyValuePair<double, int>(Score, j));
                HowManyResults++;
            }  
            else RankingList.Add(new KeyValuePair<double, int> (Score, j));
        }

        RankingList.Sort((x,y) => x.Key.CompareTo(y.Key));
        RankingList.Reverse();
    }

    private double CalculateScore(Dictionary<string,double> Vector, Dictionary<string,
    double> query) {
        
        // Usando la formula de similitud de vectores donde a la multiplicacion escalar de estos
        // la dividiremos entre el producto de los modulos de ambos vectores

        double ScalarProduct = 0;
        double ModVector = 0;
        double ModQuery = 0;
        foreach(KeyValuePair<string,double> kvp in Vector) {
            ScalarProduct += (kvp.Value * query[kvp.Key]);
            ModVector += (kvp.Value * kvp.Value);
            ModQuery += (query[kvp.Key] * query[kvp.Key]);
        }

        if(ModVector == 0) return 0;
        return ScalarProduct / ( Math.Sqrt(ModVector) * Math.Sqrt(ModQuery) );
        
    }
    
    private string GiveASuggestion(AllDocuments Docs) {
        string theSug = "";

        foreach(var kvp in qTFIDF) {
            if(kvp.Value == 0.4) {
                var ListOfPossibleSugerences = new Dictionary<string,double>();
                // Buscaremos sugerencias para esa palabra hasta un valor de diferencia de 
                // edicion (EditDistance = 5)
                
                // Si encontramos alguna guardamos cual tiene el mayor score
                // MaxScoreIDF = 0 ; MinScoreIDF = double.MaxValue;
                double MinScoreIDF = double.MaxValue;
                for(int j=1; j<=5; j++) {
                    foreach(var kvp1 in Docs.IDF) {
                        if(EditDistance(kvp.Key, kvp1.Key) == j) {
                            ListOfPossibleSugerences.Add(kvp1.Key, kvp1.Value);
                            MinScoreIDF = Math.Min(MinScoreIDF, kvp1.Value);
                        }
                    }
                    if(ListOfPossibleSugerences.Count() > 0){
                        break;
                    } 
                }

                // Si existe alguna sugerencia, la agregamos a nuestra string sugerencias
                if(ListOfPossibleSugerences.Count() > 0) {
                    foreach(var kvp2 in ListOfPossibleSugerences){
                        if(kvp2.Value == MinScoreIDF) {
                            theSug += kvp2.Key;
                            theSug += " ";
                            break;
                        }
                    }
                }
                else {
                    theSug += kvp.Key;
                    theSug += " ";
                }
            }
            else {
                theSug += kvp.Key;
                theSug += " ";
            }
        }

        return theSug;
    }
    int EditDistance(string source, string target){
        if(string.IsNullOrEmpty(source)){
            if(string.IsNullOrEmpty(target)) return 0;
            return target.Length;
        }
        if(string.IsNullOrEmpty(target)) return source.Length;

        if(source.Length > target.Length){
            var temp = target;
            target = source;
            source = temp;
        }
    
        var m = target.Length;
        var n = source.Length;
        var distance = new int[2, m + 1];
        // Initialize the distance matrix
        for(var j = 1; j <= m; j++) distance[0, j] = j;

        var currentRow = 0;
        for(var i = 1; i <= n; ++i){
            currentRow = i & 1;
            distance[currentRow, 0] = i;
            var previousRow = currentRow ^ 1;
            for(var j = 1; j <= m; j++) {
                var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                distance[currentRow, j] = Math.Min(Math.Min(
                            distance[previousRow, j] + 1,
                            distance[currentRow, j - 1] + 1),
                            distance[previousRow, j - 1] + cost);
            }
        }
        return distance[currentRow, m];
    }

    private void AppliAster(Dictionary<string,double> qTFIDF) {
        foreach(var kvp in OperAster) {
            if(qTFIDF.ContainsKey(kvp.Key)) {
                qTFIDF[kvp.Key] += (OperAster[kvp.Key] / 10 ) * 0.2;
            }
        }
    }
}

