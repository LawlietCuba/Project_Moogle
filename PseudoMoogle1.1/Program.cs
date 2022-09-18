using MyMoogleEngine;

var Directorio = new MyDirectory();

var Documentos = new AllDocuments(Directorio.GetDirectories());

System.Console.WriteLine("Hola. Introduzca su busqueda");
string query = Console.ReadLine();

Query Results = new Query(query, Documentos);

var Ranking = Results.GetRankingList();
int count = Results.GetHowManyResults();

foreach(var v in Ranking) {
    if(v.Key != 0) {
        System.Console.WriteLine(Documentos.TheDocuments[v.Value].GetTitle() + " " + v.Key);
        System.Console.WriteLine(Results.GetSnippet(v.Value));
    }
}
System.Console.WriteLine("Quizas quisiste decir " + Results.GetSuggestion());