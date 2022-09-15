using MyMoogleEngine;

var Directorio = new MyDirectory();

var Documentos = new AllDocuments(Directorio.GetDirectories());

System.Console.WriteLine("Hola. Introduzca su busqueda");
string query = Console.ReadLine();

Query Results = new Query(query, Documentos);

var Ranking = Results.GetRankingList();
int count = Results.GetHowManyResults();

foreach(var kvp in Results.GetQTFIDF()) {
    System.Console.WriteLine(kvp.Key + " " + kvp.Value);
}

foreach(var v in Ranking) {
    if(count-- > 0){
        //System.Console.WriteLine(Documentos.TheDocuments[v.Value].GetTitle() + " " + v.Key);
    }
    else break;
}
//System.Console.WriteLine("Quizas quisiste decir " + Results.GetSuggestion());

foreach(string str in Results.GetOperExcla()) {
    //System.Console.WriteLine(str);
}