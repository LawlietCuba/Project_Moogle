namespace MyMoogleEngine;

using System.Text;
using System.Globalization;
public class Document 
{
    private string DocDirectory;
    private string Text;
    private string Title;
    public string[] SplittedTextModified;
    private Dictionary <string,string> ? OriginalWithNormalized;
    public Dictionary<string,double> TF;

    public Document(string Directory) {
        this.DocDirectory = Directory;
        this.Text = ReadText(Directory);
        this.Title = GetTitle(Directory);
        this.SplittedTextModified= FirstSplit(this.Text);      
        this.TF = MakeTF(this.SplittedTextModified);
    }

    static public string GetTitle(string Directory) {
        int indexContent = Directory.LastIndexOf('/');
        int indexTxt = Directory.LastIndexOf('.');

        string Title = new string(Directory.Substring(indexContent + 1,  indexTxt - indexContent - 1));

        return Title;
    }

    static private string ReadText(string Directory) {
        string ReadText = File.ReadAllText(Directory);
        return ReadText;
    }  

    private string[] FirstSplit(string Text) {  
        // TODO: Tildes ------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------      
        string[] FirstSplit = Text.Split();

        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '~', '`', '-', '(', ')','#','@',
        '"','$', '%', '^', '&', '*', '_', '+', '=', '<', '>','0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        string[] subsequences = Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        string[] modified = new string[subsequences.Length];
        string[] finalModified = new string[subsequences.Length];
        for(int i=0; i<modified.Length; i++) {
            modified[i] = subsequences[i].ToLower();
            finalModified[i] = RemoveDiacritics(modified[i]);
        }   

        return finalModified;
    }

    static string RemoveDiacritics(string text) 
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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

        foreach(KeyValuePair <string, double> kvp in TermFrequency){
            TermFrequency[kvp.Key] = (kvp.Value/MaxTF);
        }

        return TermFrequency;
    }

    public Dictionary<string,string> MatchOrigStringWithNormString(string[] FirstSplit, string[]
    ModifiedSplit){
        
        var MatchedString = new Dictionary<string,string>();
        for(int i = 0; i<ModifiedSplit.Length; i++) {
            if(!MatchedString.ContainsKey(ModifiedSplit[i])){
                MatchedString.Add(ModifiedSplit[i], FirstSplit[i]);
            }
        }

        return MatchedString;
    }

    public void GetSplitModText() {
        foreach(string str in SplittedTextModified) {
            System.Console.Write(str + " ");
        }
        System.Console.WriteLine();
    }

}
