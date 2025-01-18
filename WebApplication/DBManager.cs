using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
public class DBManager {
    private SqliteConnection? connection = null;
    
    private string HashPassword(string password){
        using (var algorithm = SHA256.Create()) {
            var bytes_hash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }
    public bool ConnectToDB(string path) {
        Console.WriteLine("Connecting to database...");

        try 
        {
            connection = new SqliteConnection("Data Source=" + path);
            connection.Open();

            if (connection.State != System.Data.ConnectionState.Open){
                Console.WriteLine("Failed to connect");
                return false;
            }
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return false;
        }
        Console.WriteLine("Connection sucessfull");
        return true;
    }

    public void DisConnect() {
        if (null == connection)
            return;

        if (connection.State != System.Data.ConnectionState.Open){
            return;
        }
        connection.Close();
        Console.WriteLine("Discoonected from database");
    }

    public bool AddUser(string login, string password) {
        if (null == connection)
            return false;

        if (connection.State != System.Data.ConnectionState.Open){
            return false;
        }
        string REQUEST = "INSERT INTO users1 (Login, Password) VALUES ('" + login + "', '" + HashPassword(password) + "')";
        var command = new SqliteCommand(REQUEST, connection);

        int result = 0;
        try{
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return false;
        }

        if (1 == result)
            return true;
        else 
            return false;
    }

    public bool CheckUser(string login, string password) {
         if (null == connection)
            return false;
        if (connection.State != System.Data.ConnectionState.Open){
            return false;
        }

        string REQUEST = "SELECT Login,Password FROM users WHERE Login='" + login + "' AND Password = '" + HashPassword(password) + "'";
        var command = new SqliteCommand(REQUEST, connection);

        try{
            var reader = command.ExecuteReader();

            if (reader.HasRows){
                return true;
            }
            else
                return false;
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool UpdatePassword(string login, string newPassword) {
    if (null == connection)
        return false;

    if (connection.State != System.Data.ConnectionState.Open)
        return false;

    string REQUEST = "UPDATE users SET Password = @newPassword WHERE Login = @login";
    
    using var command = new SqliteCommand(REQUEST, connection);
    command.Parameters.AddWithValue("@newPassword", HashPassword(newPassword));
    command.Parameters.AddWithValue("@login", login);

    int result;
    
    try {
        result = command.ExecuteNonQuery();
    } catch (Exception exp) {
        Console.WriteLine(exp.Message);
        return false;
    }

    return result == 1; // Возвращаем true если обновление прошло успешно
}
   
    public void AddTextEntry(string inputText) {
    using var command = new SqliteCommand("INSERT INTO TextEntries (InputText) VALUES (@inputText)", connection);
    command.Parameters.AddWithValue("@inputText", inputText);
    command.ExecuteNonQuery();
}
    public void UpdateTextEntry(int id, string newText) {
    using var command = new SqliteCommand("UPDATE TextEntries SET InputText = @newText WHERE Id = @id", connection);
    command.Parameters.AddWithValue("@newText", newText);
    command.Parameters.AddWithValue("@id", id);
    command.ExecuteNonQuery();
}
public void DeleteTextEntry(int id) {
    using var command = new SqliteCommand("DELETE FROM TextEntries WHERE Id = @id", connection);
    command.Parameters.AddWithValue("@id", id);
    command.ExecuteNonQuery();
}
public string GetTextById(int id) {
    using var command = new SqliteCommand("SELECT InputText FROM TextEntries WHERE Id = @id", connection);
    command.Parameters.AddWithValue("@id", id);

    using var reader = command.ExecuteReader();
    if (reader.Read()) {
        return reader.GetString(0); // Предполагается, что текст находится в первом столбце
    }
    return null; // Если запись не найдена
}
public List<string> GetAllTexts() {
    var texts = new List<string>();
    using var command = new SqliteCommand("SELECT InputText FROM TextEntries", connection);
    
    using var reader = command.ExecuteReader();
    while (reader.Read()) {
        texts.Add(reader.GetString(0)); // Добавляем каждый текст в список
    }
    return texts;
}


    // Метод для получения текста по Id
    public TextEntry GetTextEntryById(int id)
    {
        using var command = new SqliteCommand("SELECT InputText FROM TextEntries WHERE Id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        using var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            return new TextEntry
            {
                Id = id,
                InputText = reader.GetString(0)
            };
        }
        return null;
    }

    // Метод для обновления текста по Id
    public void UpdateEncryptedText(int id, string inputText)
    {
        using var command = new SqliteCommand("UPDATE TextEntries SET InputText = @inputText WHERE Id = @id", connection);
        command.Parameters.AddWithValue("@inputText", inputText);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }
    public class TextEntry
{
    public int Id { get; set; }
    public string InputText { get; set; }
}
   public void AddRequestHistory(string username, string requestType, string requestData) {
    using var command = new SqliteCommand("INSERT INTO RequestHistory (Username, RequestType, RequestData) VALUES (@username, @requestType, @requestData)", connection);
    command.Parameters.AddWithValue("@username", username);
    command.Parameters.AddWithValue("@requestType", requestType);
    command.Parameters.AddWithValue("@requestData", requestData);
    command.ExecuteNonQuery();
}

public List<RequestHistoryEntry> GetRequestHistory(string username) {
    var history = new List<RequestHistoryEntry>();
    using var command = new SqliteCommand("SELECT RequestType, RequestData FROM RequestHistory WHERE Username = @username ORDER BY RequestType DESC", connection);
    command.Parameters.AddWithValue("@username", username);
    using var reader = command.ExecuteReader();
    while (reader.Read()) {
        history.Add(new RequestHistoryEntry {
            RequestType = reader.GetString(0),
            RequestData = reader.GetString(1)
        });
    }
    return history;
}

public class RequestHistoryEntry {
    public string RequestType { get; set; }
    public string RequestData { get; set; }
}public void ClearRequestHistory(string username) {
    using var command = new SqliteCommand("DELETE FROM RequestHistory WHERE Username = @username", connection);
    command.Parameters.AddWithValue("@username", username);
    command.ExecuteNonQuery();
}
}
