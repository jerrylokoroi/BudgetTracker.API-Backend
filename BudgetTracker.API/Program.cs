using System.Globalization;
using System.Text;
using BudgetTrackerApp.Data;
using BudgetTrackerApp.Data.Repository;
using BudgetTrackerApp.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//// Configure DbContext with SQL Server
//builder.Services.AddDbContext<BudgetTrackerDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure DbContext with SQLite for simplicity
builder.Services.AddDbContext<BudgetTrackerDbContext>(options =>
    options.UseSqlite("Data Source=budgettracker.db"));

// Register the repository
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAllOrigins");

// Define the API endpoints
app.MapGet("/api/transactions", (ITransactionRepository repository) =>
{
    return Results.Ok(repository.GetAll());
})
.WithName("GetAllTransactions");

app.MapGet("/api/transactions/{id}", (int id, ITransactionRepository repository) =>
{
    var transaction = repository.GetById(id);
    return transaction is not null ? Results.Ok(transaction) : Results.NotFound();
})
.WithName("GetTransactionById");

app.MapPost("/api/transactions", (BudgetTransaction transaction, ITransactionRepository repository) =>
{
    repository.Add(transaction);
    return Results.Created($"/api/transactions/{transaction.Id}", transaction);
})
.WithName("CreateTransaction");

app.MapPut("/api/transactions/{id}", (int id, BudgetTransaction transaction, ITransactionRepository repository) =>
{
    var existingTransaction = repository.GetById(id);
    if (existingTransaction is null)
    {
        return Results.NotFound();
    }

    existingTransaction.Amount = transaction.Amount;
    existingTransaction.Description = transaction.Description;
    existingTransaction.Date = transaction.Date;
    existingTransaction.Category = transaction.Category;
    existingTransaction.Type = transaction.Type;

    repository.Update(existingTransaction);
    return Results.NoContent();
})
.WithName("UpdateTransaction");

app.MapDelete("/api/transactions/{id}", (int id, ITransactionRepository repository) =>
{
    var existingTransaction = repository.GetById(id);
    if (existingTransaction is null)
    {
        return Results.NotFound();
    }

    repository.Delete(id);
    return Results.NoContent();
})
.WithName("DeleteTransaction");

app.MapGet("/api/transactions/export/csv", (ITransactionRepository repository) =>
{
    var transactions = repository.GetAll();
    var csvBuilder = new StringBuilder();

    csvBuilder.AppendLine("Id,Date,Description,Category,Type,Amount");

    foreach (var transaction in transactions)
    {
        csvBuilder.AppendLine($"{transaction.Id},{transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)},{transaction.Description},{transaction.Category},{transaction.Type},{transaction.Amount}");
    }

    var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
    return Results.File(bytes, "text/csv", "transactions.csv");
})
.WithName("ExportTransactionsToCsv");

app.Run();
