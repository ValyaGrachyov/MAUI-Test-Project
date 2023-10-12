using System.Text;
using System.Text.RegularExpressions;
using TestTask.IServices;

namespace TestTask.Services;

public class Analizator : IAnalizator
{
    private int max_prefix_length = 0;
    public string UnmodifiedText { get; set; }

    public Analizator(string unmodifiedText)
    {
        UnmodifiedText = unmodifiedText;
    }
    public string EditText(string[] text, string[] keyWords, string[] prefixes)
    {
        string result = UnmodifiedText;
        Dictionary<string, string> editedText = new Dictionary<string, string>();
        
        // Префиксы заносятся в словарь
        Dictionary<string, int> prefixDic = new Dictionary<string, int>();
        foreach (var el in prefixes)
        {
            if (el.Length  > max_prefix_length)
            {
                max_prefix_length = el.Length;
            }
            
            prefixDic.Add(el,0);
        }
        
        // Поиск слов в тексте для обновления
        foreach (var el in text)
        {
            if (keyWords.ToList().Contains(el.ToLower()))
            {
                if (el.Length > max_prefix_length)
                {
                    for (int i = 0; i <= max_prefix_length; i++)
                    {
                        if (prefixDic.ContainsKey(el.Substring(0, i).ToLower()))
                        {
                            editedText.Add(el.Insert(i, "-"), el);
                        }
                    }
                }
            }
        }
        
        // Производится изменение текста на обновленный
        foreach (var el in editedText)
        {
            result = result.Replace(el.Value,el.Key);
            
        }
        
        return result;

    }
}