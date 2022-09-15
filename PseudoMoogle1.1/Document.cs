namespace MyMoogleEngine;

using System.Text;
using System.Globalization;
public partial class Document 
{
    private string DocDirectory;
    private string Text;
    private string[] Tokens;
    private Dictionary <string, double  >  WordWithItsPos;
    private Dictionary<string,double> TF;

    public Document(string Directory) {
        this.DocDirectory = Directory;
        this.Text = ReadText(Directory);
        this.WordWithItsPos = new Dictionary<string, double>();
        this.Tokens= NormalizeText(this.Text);  
        CalculaAveragePositionOfEachWord(); 
        this.TF = MakeTF(this.Tokens);
    }

    public string GetTitle() {
        int indexContent = DocDirectory.LastIndexOf('/');
        int indexTxt = DocDirectory.LastIndexOf('.');

        string Title = new string(DocDirectory.Substring(indexContent + 1,  indexTxt - indexContent - 1));

        return Title;
    }

    private string ReadText(string Directory) {
        string ReadText = File.ReadAllText(Directory);
        return ReadText;
    }  

    public string[] GetTokens() {
        return Tokens;
    }

    public Dictionary<string,double> GetTF() {
        return TF;
    }

    public Dictionary<string,double> GetWordPos() {
        return WordWithItsPos;
    }
}


// Metodos mas complejos
public partial class Document
{
    private string[] NormalizeText(string Text) {  
        // TODO: Tildes ------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------      

        char[] separators = new char[] { ' ', '.', ',', ':', ';', '/', '~', '`', '-', '(', ')',
        '#','@', '"','$', '%', '^', '&', '*', '_', '+', '=', '<', '>'};
        string[] subsequences = Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        string[] finalModified = new string[subsequences.Length];
        for(int i=0; i<subsequences.Length; i++) {
            subsequences[i] = subsequences[i].ToLower();
            
            string PalabraSinTilde = "";
            for(int j=0; j<subsequences[i].Length; j++) {
                switch(subsequences[i][j]) {
                    case 'á':
                        PalabraSinTilde+= 'a';
                        break;
                    case 'é':
                        PalabraSinTilde+= 'e';
                        break;
                    case 'í':
                        PalabraSinTilde+= 'i';
                        break;
                    case 'ó':
                        PalabraSinTilde+= 'o';
                        break;
                    case 'ú':
                        PalabraSinTilde+= 'u';
                        break;

                    default:
                        PalabraSinTilde+= subsequences[i][j];
                        break;
                }

                finalModified[i] = new string(PalabraSinTilde);
            }
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

    private void CalculaAveragePositionOfEachWord(){
        var temp = new Dictionary<string,Pair> ();
        for(int j=0; j<Tokens.Length; j++) {
            if(!temp.ContainsKey(Tokens[j])) {
                temp.Add(Tokens[j], new Pair(j, 1));
            }
            else {
                string s = Tokens[j];
                temp[s] = temp[s].Sum(j, 1);
            }
        }

        this.WordWithItsPos = new Dictionary<string, double>();
        foreach(KeyValuePair<string,Pair> sp in temp) {
            WordWithItsPos.Add(sp.Key, (temp[sp.Key].first*1.0)/ (temp[sp.Key].second *1.0));
        }
    }
    private Dictionary<string,double> MakeTF(string[] Words){
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

    public Dictionary<string,double> CreateVectorDoc(Dictionary<string,double> qTFIDF,
                                                     Dictionary<string,double> IDF) {

        var VectorDoc = new Dictionary<string,double>();

        foreach(var kvp in qTFIDF) {
            if(TF.ContainsKey(kvp.Key)) {
                VectorDoc.Add(kvp.Key, TF[kvp.Key]*IDF[kvp.Key]);
            }
            else {
                VectorDoc.Add(kvp.Key, 0);
            }
        }

        return VectorDoc;
    }
}
