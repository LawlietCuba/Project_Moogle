namespace MoogleEngine;

using System.Text.RegularExpressions;   
public class Query
{
    private string query;

    private string[] TokensQ;

    private Dictionary<string,double> qTFIDF;

    private List<KeyValuePair< double,int >> RankingList;

    private int HowManyResults;             

    private string Suggestion;

    private Dictionary<string,string> Closeness;

    private List<string> Pow;
    private List<string> Excla;

    private Dictionary<int,string> TheSnippets;

    public Query(string query, AllDocuments Docs) {
        this.query = query;
        this.TokensQ = ProcessQuery(this.query);

        this.qTFIDF = MakeQueryTFxIDF(this.TokensQ, Docs.IDF);

        // Pasemos por cada lista de operadores
        // Relevancia en orden ascendente de los operadores sera:
        // 1. Asteriscos
        // 2. Cercania
        // 3. Exclamacion
        // PD : Potencia solo nos afectara en la cantidad de documentos que devolvemos al usuario

        this.Closeness = new Dictionary<string, string>();
        this.Pow = new List<string>();
        this.Excla = new List<string>();
        DoOperators(query);

        this.RankingList = new List<KeyValuePair<double, int>>();
        Search(Docs);
        this.HowManyResults = CountResults();
        this.Suggestion = GiveASuggestion(Docs);

        this.TheSnippets = new Dictionary<int, string>();
        CreateSnippets(Docs);
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

    public Dictionary<string,double> GetQTFIDF() {
        return qTFIDF;
    }
    public string GetSnippet(int j) {
        return TheSnippets[j];
    }

    public Dictionary<int,string> GetSnippet() {
        return TheSnippets;
    }
    static private string[] ProcessQuery(string query) { 
        
        // Normalizar la query       
        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '`', '-', '(', ')','#',
        '@', '$', '%', '&', '_', '+', '=', '<', '>', '*', '^', '!', '~'};
        string[] subsequences = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        for(int i=0; i<subsequences.Length; i++) {
            subsequences[i] = TokenizeWord(subsequences[i]);           
        }
        
        return subsequences;
    }

    static string TokenizeWord(string word) {
        word = word.ToLower();

        word = word.Replace('??', 'a');
        word = word.Replace('??', 'e');
        word = word.Replace('??', 'i');
        word = word.Replace('??', 'o');
        word = word.Replace('??', 'u');  

        return word;
    }

    private void DoOperators(string text) {
        //Asteriscos

        Regex aster = new Regex(@"(\*+)\W*(\w+)");

        MatchCollection matches = aster.Matches(text);

        foreach(Match m in matches) {
            GroupCollection groups = m.Groups;
            string target = TokenizeWord(groups[2].ToString());
            qTFIDF[target] *= (1 + 0.1*groups[1].Length);
        }

        //Operador de cercania

        Regex closen = new Regex(@"(\w+)\W*~\W*(\w+)");

        matches = closen.Matches(text);

        foreach(Match m in matches) {
            GroupCollection groups = m.Groups;
            string target1 = TokenizeWord(groups[1].ToString());
            string target2 = TokenizeWord(groups[2].ToString());

            Closeness.Add(target1, target2);
        }

        // Exclamation

        Regex excla = new Regex(@"(\!+)\W*(\w+)");

        matches = excla.Matches(text);

        foreach(Match m in matches) {
            GroupCollection groups = m.Groups;
            string target = TokenizeWord(groups[2].ToString());
            Excla.Add(target);
        }

        // Pow

        Regex pow = new Regex(@"(\^+)\W*(\w+)");

        matches = pow.Matches(text);

        foreach(Match m in matches) {
            GroupCollection group = m.Groups;
            string target = TokenizeWord(group[2].ToString());
            Pow.Add(target);
        }
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

        // foreach(var kvp in qTFIDF) {
        //     if(kvp.Key.Length <= 2 && qTFIDF.Count > 1) {
        //         qTFIDF[kvp.Key] = 0;
        //     }
        // }

        return qTFIDF;
    }
    
    private double CalculateTFxIDF(double freq, double idf) {
        double a = 0.4;

        return (a + (1 - a) * freq * idf);
    }

    private void Search(AllDocuments Docs) {
        for(int j = 0; j<Docs.TheDocuments.Length; j++){
            // Creamos el vector correspondiente a la query de cada documento
            var CorresVector = Docs.TheDocuments[j].CreateVectorDoc(qTFIDF, Docs.IDF);
            // Calculamos el score
            double Score = CalculateScore(CorresVector, qTFIDF);
            if(Score > 0){
                // Si el score es distinto de cero significa que al menos una de las palabras de la
                // query estan en el documento

                // Aqui aplicaremos el operador de cercania

                foreach(var kvp in Closeness) {
                    
                    var lis1 = AllIndexesOf(Docs.TheDocuments[j].GetText(), kvp.Key);
                    var lis2 = AllIndexesOf(Docs.TheDocuments[j].GetText(), kvp.Value);

                    double Min_Dist = int.MaxValue;
                    foreach(int x in lis1) {
                        foreach(int y in lis2) {
                            Min_Dist = Math.Min(Math.Abs(x-y), Min_Dist);
                        }
                    }

                    // Utilizando la funcion 1/x...
                    if(Min_Dist != int.MaxValue) {
                        // 10 es escogido arbitrariamente para ser la distancia promedio entre 
                        // dos palabras que deberian estar juntas en el texto segun la busqueda
                        // Podria hacerse una AI por progresion lineal que de un valor aproximado
                        // usando linear_regresion


                        Score *= (1+10/Min_Dist);
                        
                    }
                }                

                foreach(string s in Excla) {
                    Regex exc = new Regex(@"\s+" + s + @"\s+", RegexOptions.IgnoreCase);
                    Match match = exc.Match(Docs.TheDocuments[j].GetText());
                    if(match.Success) {
                        Score = 0;
                        break;  
                    }
                }

                if(Score != 0) {
                    RankingList.Add(new KeyValuePair<double, int>(Score, j));
                    HowManyResults++;
                } 
            }  
        }

        RankingList.Sort((x,y) => x.Key.CompareTo(y.Key));
        RankingList.Reverse();
    }

    private List<int> AllIndexesOf(string str, string value) {
        if (String.IsNullOrEmpty(value))
            throw new ArgumentException("the string to find may not be empty", "value");
        List<int> indexes = new List<int>();
        for (int index = 0;; index += value.Length) {
            index = str.IndexOf(value, index);
            if (index == -1)
                return indexes;
            indexes.Add(index);
        }
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

        double value = ScalarProduct / ( Math.Sqrt(ModVector) * Math.Sqrt(ModQuery) );

        // Ahora apliquemos el operador de potencia

        if(Pow.Count != 0) {
            foreach(string s in Pow) {
                if(Vector[s] == 0) {
                    value = 0;
                }
            }
        }

        return value;
    }
    
    private string GiveASuggestion(AllDocuments Docs) {
        string theSug = "";
        string theOriginal = "";

        foreach(var kvp in qTFIDF) {
            theOriginal += kvp.Key;
            theOriginal += " ";
        }

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
        
        return theSug==theOriginal ? "Lo sentimos, no hay resultados para su b??squeda" : "??Quiz??s quisiste decir " + theSug + "?";
    }

    private void CreateSnippets(AllDocuments Docs) {
        foreach(var kvp in RankingList) {
            if(kvp.Key == 0) continue;
            string text = Docs.TheDocuments[kvp.Value].GetText();

            // Para elaborar el snippet, se tomara la primera palabra mas
            // importante de la query de la query con la segunda palabra mas relevate de la query
            // y si ambas aparecen en el documento se buscara elaborar el snippet con ellas
            // Si no aparecen dos palabras de la query en el documento se repetira el mismo proceso
            // pero con una sola palabra


            bool GotSnippet = false;
            // Dos palabras
            double val1 = 0;
            string str1 = "";
            
            var check1 = new List<string>();
            foreach(var kv in qTFIDF) {
                if(check1.Contains(kv.Key)) continue;
                if(val1 < kv.Value) {
                    str1 = kv.Key;
                    val1 = kv.Value;
                }
                else continue;

                val1 = 0;
                check1.Add(str1);
                
                double val2 = 0;
                string str2 = "";
                var check2 = new List<string>();
                foreach(var kv2 in qTFIDF) {
                    if(check1.Contains(kv2.Key) || check2.Contains(kv2.Key)) continue;
                    if(val2 < kv2.Value) {
                        str2 = kv2.Key;
                        val2 = kv2.Value;
                    }
                    else continue;

                    val2 = 0;
                    check2.Add(str2);

                    if(str1 != "" && str2 != "") {
                        Regex snippet = new Regex(@"(\w+\W+){0,2}" + (str1) + @"\s+.*" + (str2) + @"(\W+\w+){0,40}", RegexOptions.IgnoreCase);

                        Match match = snippet.Match(text);

                        if(match.Success){
                            TheSnippets.Add(kvp.Value, match.ToString());
                            GotSnippet = true;
                            break;
                        }

                        Regex snippet2 = new Regex(@"(\w+\W+){0,2}" + (str2) + @"\s+.*" + (str1) + @"(\W+\w+){0,40}", RegexOptions.IgnoreCase);

                        Match match2 = snippet2.Match(text);

                        if(match2.Success){
                            TheSnippets.Add(kvp.Value, match2.ToString());
                            GotSnippet = true;
                            break;
                        }

                        // Si no encuentra juntas dichas palabras entonces combinaremos pedazos de texto donde aparezcan
                        // ambas

                        string CombinedSnippet = "";

                        Regex snippet4 = new Regex(@"(\w+\W+){0,2}"+ str1 + @"(\W+\w+){0,40}", RegexOptions.IgnoreCase);

                        Match match4 = snippet4.Match(text);

                        Regex snippet5 = new Regex(@"(\w+\W+){0,2}"+ str2 + @"(\W+\w+){0,40}", RegexOptions.IgnoreCase);

                        Match match5 = snippet5.Match(text);

                        if(match4.Success && match5.Success) {
                            if(match4.Index <= match5.Index) {
                                CombinedSnippet += (match4.ToString() + " (...) " + match5.ToString());
                            }
                            else {
                                CombinedSnippet += (match5.ToString() + " (...) " + match4.ToString());
                            }

                            TheSnippets.Add(kvp.Value, CombinedSnippet);
                            GotSnippet = true;
                            break;
                        }

                    }
                }

                if(GotSnippet == true) break;
                else {
                    // Una palabra
                    var check3 = new List<string>();
                    double val3 = 0;
                    string str3 = "";
                    
                    foreach(var kv3 in qTFIDF) {
                        if(check3.Contains(kv3.Key)) continue;
                        if(val3 < kv3.Value) {
                            str3 = kv3.Key;
                            val3 = kv3.Value;
                        }
                        else continue;
                        
                        val3 = 0;
                        check3.Add(kv3.Key);

                    
                        if(str3 != "" ) {
                            Regex snippet3 = new Regex(@"(\w+\W+){0,2}"+ str3 + @"(\W+\w+){0,40}", RegexOptions.IgnoreCase);

                            Match match3 = snippet3.Match(text);

                            if(match3.Success){
                                TheSnippets.Add(kvp.Value, match3.ToString());
                                GotSnippet = true;
                                break;
                            }
                        }
                        if(GotSnippet == true) break;
                    }
                }

                if(GotSnippet == true) break;
            } 
        }
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
    
        int m = target.Length;
        int n = source.Length;
        var distance = new int[2, m + 1];
        // Initialize the distance matrix
        for(int j = 1; j <= m; j++) distance[0, j] = j;

        var currentRow = 0;
        for(int i = 1; i <= n; ++i){
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

    private int CountResults() {
        int count = 0;
        foreach(var kvp in RankingList) {
            if(kvp.Key != 0) count++;
        }

        return count;
    }
}

