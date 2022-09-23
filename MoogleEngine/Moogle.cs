namespace MoogleEngine;

public class Moogle
{
    public static SearchResult Query(string query) {

        var Directorio = new MyDirectory();

        var Docs = new AllDocuments(Directorio.GetDirectories());

        Query quer = new Query(query, Docs);
        
        SearchItem[] items = new SearchItem[quer.GetRankingList().Count];

        int cont = 0;   
        foreach(var kvp in quer.GetRankingList()) {
                if(kvp.Key != 0) {
                    items[cont] = new SearchItem(Docs.TheDocuments[kvp.Value].GetTitle(), 
                    quer.GetSnippet(kvp.Value), (float)kvp.Key);
                    cont++;
                }
        }

        SearchResult ssult = new SearchResult();

        if(items.Length == 0) {
            ssult = new SearchResult(items, quer.GetSuggestion());
        }
        else  {
            ssult = new SearchResult(items, "");
        }

        return ssult;
    }    
}
