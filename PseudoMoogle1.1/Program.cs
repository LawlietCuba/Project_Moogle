using MyMoogleEngine;

var Directorio = new MyDirectory();

var Documentos = new AllDocuments(Directorio.GetDirectories());

System.Console.WriteLine("Hola. Introduzca su busqueda");
string query = Console.ReadLine();

Query Results = new Query(query, Documentos);

var Ranking = Results.GetRankingList();
int count = Results.GetHowManyResults();

int AmountToShow = Math.Min(count, 10);
int counter = 1;

foreach(var v in Ranking) {
    if(v.Key != 0) {
        System.Console.WriteLine(counter + ". " + Documentos.TheDocuments[v.Value].GetTitle() + " " + v.Key);
        if(Results.GetSnippet().ContainsKey(v.Value)) {
            System.Console.WriteLine(Results.GetSnippet(v.Value));
        }
    }
    counter++;
    if(counter > AmountToShow) break;
}

// if(counter <= 10) {
    System.Console.WriteLine("Quizas quisiste decir " + Results.GetSuggestion());
// }