namespace MoogleEngine;

public class Preprocess {
    public MyDirectory directory;
    public AllDocuments Docs;

    public Preprocess (){
        this.directory = new MyDirectory();
        this.Docs = new AllDocuments(directory.GetDirectories());
    }
}

public class Moogle
{

    public static Preprocess prepro;
    public static SearchResult Query(string query) {

        Query quer = new Query(query, prepro.Docs);
        
        SearchItem[] items = new SearchItem[quer.GetRankingList().Count];

        int cont = 0;   
        foreach(var kvp in quer.GetRankingList()) {
                if(kvp.Key != 0) {
                    items[cont] = new SearchItem(prepro.Docs.TheDocuments[kvp.Value].GetTitle(), 
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
