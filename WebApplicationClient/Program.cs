using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

CookieContainer cookies = new CookieContainer();
HttpClientHandler handler = new HttpClientHandler();
HttpClient client = new HttpClient(handler);
handler.CookieContainer = cookies;

const string DEFAULT_SERVER_URL = "http://localhost:5000";
client.BaseAddress = new Uri(DEFAULT_SERVER_URL);

void LogInOnServer(string? username, string? password)
{
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        Console.WriteLine("Логин или пароль не могут быть пустыми");
        return;
    }

    string request = "/login?login=" + username + "&password=" + password;
    var response = client.PostAsync(request, null).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Авторизация прошла успешно");
    }
    else 
    {
        Console.WriteLine("Ошибка авторизации");
    }
    MainMenu();
}

void AddText(string? text)
{
    string request = "/add-text";

    var json_data = new {
        InputText = text
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст успешно добавлен");
    }
    else
    {
       Console.WriteLine("Ошибка добавления текста");
    }
}

void SignUp(string username, string password)
{
    string request = "/signup?login=" + username + "&password=" + password;
    var response = client.PostAsync(request, null).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Вы успешно зарегестрировались");
    }
    else 
    {
        Console.WriteLine("Ошибка регистрации");
    }
}

void EncryptText(string _ID)
{
    string request = "/encrypt-text";

    var json_data = new {
        ID = _ID
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст успешно зашифрован");
    }
    else
    {
        Console.WriteLine("Ошибка шифрования текста");
    }
}

void DecryptText(string _ID)
{
     string request = "/encrypt-text";

    var json_data = new {
        ID = _ID
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    
    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст успешно расшифрован");
    }
    else
    {
        Console.WriteLine("Ошибка расшифровки текста");
    }
}

string ShowHistory()
{
    string request = "/history";

    var requestMessage = new HttpRequestMessage(HttpMethod.Get, request){};
    
    var response = client.SendAsync(requestMessage).Result;
    if (response.IsSuccessStatusCode)
    {
        return response.Content.ReadAsStringAsync().Result;
    }
    else
    {
        return "Ошибка получения истории запросов";
    }
}

string DeleteHistory()
{
    string request = "/delete-history";

    var requestMessage = new HttpRequestMessage(HttpMethod.Delete, request){};
    
    var response = client.SendAsync(requestMessage).Result;
    if (response.IsSuccessStatusCode)
    {
        return "История удалена";
    }
    else
    {
        return "Ошибка удаления истории запросов";
    }
}

void ChangePassword(string oldPassword, string _newPassword)
{
     string request = "/change-password";

    var json_data = new {
        currentPassword = oldPassword,
        newPassword = _newPassword
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    
    var response = client.PatchAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Пароль изменен");
    }
    else
    {
        Console.WriteLine("Ошибка изменения пароля");
    }
}

void UpdateText(int _id, string _newText)
{
    string request = "/update-text";

    var json_data = new {
        Id = _id,
        NewText = _newText
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PatchAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст успешно обновлен");
    }
    else
    {
       Console.WriteLine("Ошибка обновления текста");
    }
}

void DeleteText(int _id)
{
    string request = "/delete-text";

    var json_data = new {
        Id = _id
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var requestMessage = new HttpRequestMessage(HttpMethod.Delete, request){
        Content = content
    };
    
    var response = client.SendAsync(requestMessage).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Текст удален");
    }
    else
    {
       Console.WriteLine("Ошибка удаления текста");
    }
}

string GetText(int _id)
{
    string request = "/get-text";

    var json_data = new {
        Id = _id
        };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var requestMessage = new HttpRequestMessage(HttpMethod.Get, request){
        Content = content
    };
    
    var response = client.SendAsync(requestMessage).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Полученный текст: ");
        return response.Content.ReadAsStringAsync().Result;
    }
    else
    {
       return "Ошибка получения текста";
    }
}

string GetAllTexts()
{
    string request = "/get-all-texts";

    var requestMessage = new HttpRequestMessage(HttpMethod.Get, request){};
    
    var response = client.SendAsync(requestMessage).Result;
    if (response.IsSuccessStatusCode)
    {
        return response.Content.ReadAsStringAsync().Result;
    }
    else
    {
        return "Ошибка получения текстов";
    }
}

void ShowMenu()
{
    Console.WriteLine("\nВыберите действие:");
    Console.WriteLine("1. Регистрация");
    Console.WriteLine("2. Просмотреть историю запросов");
    Console.WriteLine("3. Удаление истории запросов");
    Console.WriteLine("4. Изменение пароля");
    Console.WriteLine("5. Добавление текста");
    Console.WriteLine("6. Обновление текста");
    Console.WriteLine("7. Удаление текста");
    Console.WriteLine("8. Получение текста по ID");
    Console.WriteLine("9. Получение всех текстов");
    Console.WriteLine("10. Шифрование текста");
    Console.WriteLine("11. Расшифрование текста");
    Console.WriteLine("0. Выход");
}

void MainMenu()
{
   while (true)
   {
       ShowMenu();
       
       string? choice = Console.ReadLine();

       switch (choice)
       {
           case "1":
               Console.WriteLine("Введите логин и пароль для регистрации: ");
               string? newUsername = Console.ReadLine();
               string? newPassword = Console.ReadLine();
               SignUp(newUsername, newPassword);
               break;

            case "2":
                Console.WriteLine(ShowHistory());
                break;

           case "3":
               Console.WriteLine(DeleteHistory());
               break;

           case "4":
               Console.WriteLine("Введите старый и новый пароль: ");
               string? oldPassword = Console.ReadLine();
               string? updatedPassword = Console.ReadLine();
               ChangePassword(oldPassword, updatedPassword);
               break;

           case "5":
                Console.WriteLine("Введите текст: ");
                string? InputText = Console.ReadLine();
                AddText(InputText);
                break;

           case "6":
               Console.WriteLine("Введите ID текста для обновления и новый текст: ");
               int updateId = int.Parse(Console.ReadLine());
               string? updatedText = Console.ReadLine();
               UpdateText(updateId, updatedText);
               break;

           case "7":
               Console.WriteLine("Введите ID текста для удаления: ");
               int deleteId = int.Parse(Console.ReadLine());
               DeleteText(deleteId);
               break;

           case "8":
               Console.WriteLine("Введите ID текста для получения: ");
               int getId = int.Parse(Console.ReadLine());
               Console.WriteLine(GetText(getId));
               break;

           case "9":
               Console.WriteLine(GetAllTexts());
               break;

           case "10":
               Console.WriteLine("Введите ID текста для шифрования: ");
               string? textToEncrypt = Console.ReadLine();
               EncryptText(textToEncrypt);
               break;

           case "11":
               Console.WriteLine("Введите ID текста для расшифрования: ");
               string? textToDecrypt = Console.ReadLine();
               DecryptText(textToDecrypt);
               break;

           case "0":
               return; // Выход из меню

           default:
               Console.WriteLine("Неверный выбор. Пожалуйста, попробуйте снова.");
               break;
       }
   }
}

try
{
   // Авторизация
   Console.WriteLine("Введите логин и пароль: ");
   string? username = Console.ReadLine();
   string? password = Console.ReadLine();
   LogInOnServer(username, password);
}
catch (Exception exp)
{
   Console.WriteLine("Ошибка: " + exp.Message);
}