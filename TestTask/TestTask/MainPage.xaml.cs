using System.Globalization;
using System.Net.Mime;
using System.Text;
using Aspose.Cells;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using TestTask.IServices;
using TestTask.Services;

namespace TestTask;

public partial class MainPage : ContentPage
{
    private string[] main_text { get; set; }
    private string[] key_words { get; set; }
    private string[] prefixes { get; set; }
    
    private string unchanged_text { get; set; }
    
    private int max_word_length = 0;
    private int max_text_length = 0;

    private string[] white_words = new string[]
        {"а", "б", "в","г","д","з","л","м","н","п","р","т","ф","х","ц","ч","ш","щ","ъ","ь", "е", "ё", "ж", "и","й", "к", "о", "с", "у", "ы", "э", "ю", "я"};

    
    public MainPage()
    {
        InitializeComponent();
    }

    // Функция загрузки файла с ключевыми словами
    private async void Key_wordClicked(object sender, EventArgs e)
    {
        
        FileResult result = await FilePicker.PickAsync();

        if (result == null) return;

        if (result.ContentType != "text/plain")
        {await DisplayAlert("Внимание",
                    "Вы загрузили файл с ключевыми словами не в формате .txt","Принято");
            return;
        }
        
        // Здесь происходит парсинг данных из файла
        key_words = await ReadFile(await result.OpenReadAsync());
        
        for(int i = 0; i< key_words.Length; i++)
        {
            key_words[i] = key_words[i].ToLower();
            foreach (var x in white_words)
            {
                if (key_words[i] == x) ;
            }
        }

        foreach (var el in key_words)
        {
            if (el.Length > max_word_length)
            {
               await DisplayAlert("Внимание","Длина одного из слов не соответствует введеной величине","Принято");
               return;
            }
        }
        
        // Элементу Label на основном экране присваивается название файла
        KeyWordLabel.Text = result.FileName;
    }
    
    // Функция  загрузки файла с приставками 
    private async void Prefixes_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();
        
        if (result == null) return;
        
        if (result.ContentType != "text/plain")
        {
            await DisplayAlert("Внимание",
                "Вы загрузили файл с приставками не в формате .txt","Принято");
            return;
        }
        
        // Здесь происходит парсинг данных из файла
        prefixes = await ReadFile(await result.OpenReadAsync());
        
        foreach (var el in prefixes)
        {
            if (el.Length > 5)
            {
                await DisplayAlert("Внимание", "Одна из приставок не соответствует требованиям", "Принято");
                return;
            }
            
        }

        // Элементу Label на основном экране присваивается название файла
        PrefixLabel.Text = result.FileName;
    }
    
    // Функция загрузки файла с осоновным текстом
    private async void Text_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();
        
        if (result == null) return;
        
        if (result.ContentType != "text/plain")
        {
            await DisplayAlert("Внимание",
                "Вы загрузили файл с текстом не в формате .txt","Принято");
            return;
        }
        
        // Здесь происходит парсинг данных из файла
        main_text = await Text_Parser(await result.OpenReadAsync());

        if (unchanged_text.Length > max_text_length)
        {
            await DisplayAlert("Внимание", "Текст не соответствует длине", "Принято");
            return;
        }
        
        // Элементу Label на основном экране присваивается название файла
        TextLabel.Text = result.FileName;
    }
    
    // Функция запускающая BL(Business logic )
    private async void Text_AnalizeClicked(object sender, EventArgs e)
    {
        if (main_text == null || key_words == null || prefixes == null)
        {
            await DisplayAlert("Внимание", "Один из файлов не загружен или не написан текст", "Принято");
            return;
        }
        
        // Создается экземпляр класса, в котором реализован основной алгоритм
        var newText = new Analizator(unchanged_text);
        var updatedText = newText.EditText( main_text,key_words,prefixes);
        
        // Выходные данные передаются на главную страницу
        Final_Main_Text.Text = unchanged_text;
        Key_Words.Text = await Create_String(key_words);
        Prefixes.Text = await Create_String(prefixes);
        Edited_Text.Text = updatedText;
    }

    // Функция считывания файлов и парсинга файлов с приставками и ключевыми словами
    private async Task<string[]> ReadFile(Stream stream)
    {
        byte[] buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer,0,buffer.Length);
        var text =  Encoding.UTF8.GetString(buffer);
        text = text.Remove(text.Length - 1);
        return text.Split(',');
    }

    // Функция для перевода массива в строку
    private async Task<string> Create_String(string[] arr)
    {
        string result = "";
        foreach (var el in arr)
        {
            result += el + ",";
        }

        return result;
    }

    // Функция для установки максимальной длины ключевого слова
    private void Set_WordLength(object sender, TextChangedEventArgs e)
    {
        Set_Length(Word_Length.Text, ref max_word_length);
    }
    // Функция для установки максимальной длины текста в символах 
    private void Set_TextLength(object sender, TextChangedEventArgs e)
    {
        Set_Length(Text_Length.Text, ref max_text_length);
    }
    
    // Функция для проверки на ввод числа
    private  void Set_Length(string str, ref int length)
    {
        if (str == "")
        {
            return;
        }
        
        if (!Int32.TryParse(str, out length))
        {
             DisplayAlert("Внимание","Вводите число, а не строку XD ","Принято");
        }
    }

    // Функция для парсинга файла с основным текстом
    private async Task<string[]> Text_Parser(Stream stream)
    {
        byte[] buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer,0,buffer.Length);
        var text =  Encoding.UTF8.GetString(buffer);
        unchanged_text = text;
        var editStr = text.Split(' ', ',', '.', '!', '?', '"').ToList();
        

        for (int i = 0; i < editStr.Count; i++)
        {
            
            if (editStr[i] == "")
            {
                editStr.Remove("");
            }
        }

        return editStr.ToArray();
    }

    // Кнопка для перехода на раздел О приложении
    private async void About_Button_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutPage());
    }

    
}