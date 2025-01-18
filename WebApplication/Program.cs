using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

DPWebAdapter dp = new DPWebAdapter();
DBManager db = new DBManager();

app.MapGet("/", () => "Double Permutation Cipher Service.");

app.MapGet("/history", [Authorize] (HttpContext context) => {    
    var username = context.User.Identity.Name;
    var history = db.GetRequestHistory(username);
    return Results.Ok(history);
});

app.MapDelete("/delete-history", [Authorize] (HttpContext context) => {
    var username = context.User.Identity.Name;
    db.ClearRequestHistory(username);
    return Results.Ok("History cleared successfully.");
});

app.MapPost("/add-text", [Authorize] ([FromBody] TextRequest request, HttpContext context) => {
    var username = context.User.Identity.Name;

    db.AddTextEntry(request.InputText);

    db.AddRequestHistory(username, "AddText", request.InputText);

    return Results.Ok("Text added successfully.");
});

app.MapPatch("/update-text", [Authorize] ([FromBody] UpdateTextRequest request, HttpContext context) => {
    var username = context.User.Identity.Name;

    db.UpdateTextEntry(request.Id, request.NewText);

    db.AddRequestHistory(username, "UpdateText", request.NewText);

    return Results.Ok("Text updated successfully.");
});

app.MapDelete("/delete-text", [Authorize] ([FromBody] DeleteTextRequest request, HttpContext context) => {
    var username = context.User.Identity.Name;

    db.DeleteTextEntry(request.Id);

    db.AddRequestHistory(username, "DeleteText", $"Deleted text with ID: {request.Id}");

    return Results.Ok("Text deleted successfully.");
});

app.MapGet("/get-text", [Authorize] ([FromBody] ChooseTextRequest request, HttpContext context) => {
    var text = db.GetTextById(request.Id);
    if (text != null) {
        return Results.Ok(text);
    }
    return Results.NotFound("Text not found.");
});

app.MapGet("/get-all-texts", [Authorize] () => {
    var texts = db.GetAllTexts();
    return Results.Ok(texts);
});

app.MapPost("/encrypt-text", [Authorize] async ([FromBody] EncryptTextRequest request, HttpContext context) => {
    var username = context.User.Identity.Name;

    var existingEntry = db.GetTextEntryById(request.Id);
    if (existingEntry == null)
    {
        return Results.NotFound("Text entry not found.");
    }

    int[] columnOrder = { 1, 0, 2 };
    int[] rowOrder = { 0, 1, 2 };

    var cipher = new DoublePermutation();
    string encryptedText = cipher.Encrypt(existingEntry.InputText, columnOrder, rowOrder);

    db.UpdateEncryptedText(request.Id, encryptedText);

    db.AddRequestHistory(username, "EncryptText", existingEntry.InputText);

    return Results.Ok("Text encrypted successfully.");
});

app.MapPost("/decrypt-text", [Authorize] async ([FromBody] EncryptTextRequest request, HttpContext context) => {
    var username = context.User.Identity.Name;

    var existingEntry = db.GetTextEntryById(request.Id);
    if (existingEntry == null)
    {
        return Results.NotFound("Text entry not found.");
    }

    int[] columnOrder = { 1, 0, 2 };
    int[] rowOrder = { 0, 1, 2 };

    var cipher = new DoublePermutation();
    string encryptedText = cipher.Decrypt(existingEntry.InputText, columnOrder, rowOrder);

    db.UpdateEncryptedText(request.Id, encryptedText);

    db.AddRequestHistory(username, "DecryptText", existingEntry.InputText);

    return Results.Ok("Text decrypted successfully.");
});

app.MapPost("/login", async (string login, string password, HttpContext context) => {
    if (!db.CheckUser(login,password))
    {
        return Results.Unauthorized();
    }
    var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
    var claimIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimIdentity));
    return Results.Ok();
});

app.MapPost("/signup", (string login, string password) => {
    if (db.AddUser(login, password)){
        return Results.Ok("User " + login + " Registation successfull");
    }
    else {
        return Results.Problem("Registration failed user " + login);
    }
});

app.MapPatch("/change-password", async (ChangePasswordRequest request, HttpContext context) => {
    var username = context.User.Identity.Name;

    if (string.IsNullOrEmpty(username)) {
        return Results.Unauthorized();
    }

    if (!db.CheckUser(username, request.CurrentPassword)) {
        Console.WriteLine("Failed to change password");
        return Results.Unauthorized();
    }

    if (db.UpdatePassword(username, request.NewPassword)) {
        return Results.Ok("Password changed successfully.");
    }
    
    return Results.Problem("Failed to change password.");
});

const string DB_Path = "/home/Artem/Work/Databases/WebAppDB.db";

if (!db.ConnectToDB(DB_Path)) 
{
    Console.WriteLine("Connection to database failed" + DB_Path);
    Console.WriteLine("Shuting down");
    return;
}

app.Run();
db.DisConnect();

public class DPResult
{
    public DPResult(string inpt) 
    {
        Input = inpt;
    }
    public DPResult(int[] co, int[] ro) 
    {
        ColumnOrder = co;
        RowOrder = ro;
    }
    public string Input { get; set; }
    public int[] ColumnOrder { get; set; }
    public int[] RowOrder { get; set; }
}
public class DPWebAdapter {
    private DoublePermutation dp = new DoublePermutation();
         public IResult Encrypt(string input, int[] columnOrder, int[] rowOrder) {
            return Results.Ok(new DPResult(dp.Encrypt(input, columnOrder, rowOrder)));
        }

        public IResult Decrypt(string input, int[] columnOrder, int[] rowOrder) {
            return Results.Ok(new DPResult(dp.Decrypt(input, columnOrder, rowOrder)));
        }
    }

public class DoublePermutation
{
    public string Encrypt(string input, int[] columnOrder, int[] rowOrder)
    {
        return DoublePermutationCipher(input, columnOrder, rowOrder);
    }

    public string Decrypt(string input, int[] columnOrder, int[] rowOrder)
    {
        return DoublePermutationDecipher(input, columnOrder, rowOrder);
    }

    private static string DoublePermutationCipher(string input, int[] columnOrder, int[] rowOrder)
    {
        string[] words = input.Split(' ');
        string result = "";

        foreach (string word in words)
        {
            int rows = (int)Math.Ceiling((double)word.Length / columnOrder.Length);
            char[,] matrix = new char[rows, columnOrder.Length];

            for (int i = 0; i < word.Length; i++)
            {
                matrix[i / columnOrder.Length, i % columnOrder.Length] = word[i];
            }

            char[,] permutedMatrix = new char[rows, columnOrder.Length];

            for (int col = 0; col < columnOrder.Length; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (row < rows && columnOrder[col] < matrix.GetLength(1))
                        permutedMatrix[row, col] = matrix[row, columnOrder[col]];
                }
            }

            string encryptedWord = "";
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columnOrder.Length; col++)
                {
                    encryptedWord += permutedMatrix[row, col];
                }
            }
            result += encryptedWord.TrimEnd('\0') + " ";
        }

        return result.Trim();
    }

    private static string DoublePermutationDecipher(string input, int[] columnOrder, int[] rowOrder)
    {
        string[] words = input.Split(' ');
        string result = "";

        foreach (string word in words)
        {
            int rows = (int)Math.Ceiling((double)word.Length / columnOrder.Length);
            char[,] permutedMatrix = new char[rows, columnOrder.Length];

            for (int i = 0; i < word.Length; i++)
            {
                permutedMatrix[i / columnOrder.Length, i % columnOrder.Length] = word[i];
            }

            char[,] matrix = new char[rows, columnOrder.Length];

            for (int col = 0; col < columnOrder.Length; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (row < rows && columnOrder[col] < permutedMatrix.GetLength(1))
                        matrix[row, columnOrder[col]] = permutedMatrix[row, col];
                }
            }

            string decryptedWord = "";
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columnOrder.Length; col++)
                {
                    decryptedWord += matrix[row, col];
                }
            }

            result += decryptedWord.TrimEnd('\0') + " ";
        }

        return result.Trim();
    }
}

public class EncryptionRequest
{
    public string Input { get; set; }
    public int[] ColumnOrder { get; set; }
    public int[] RowOrder { get; set; }
}
public class ChangePasswordRequest {
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
   public class TextEntry
   {
       public int Id { get; set; }
       public string InputText { get; set; }
   }
   public class TextRequest
{
    public string InputText { get; set; }
}
public class UpdateTextRequest
{
    public int Id { get; set; }
    public string NewText { get; set; }
}
public class DeleteTextRequest
{
    public int Id { get; set; }
}
public class ChooseTextRequest
{
    public int Id { get; set; }
}
public class EncryptTextRequest
{
    public int Id { get; set; }
}
