using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

var todos = new List<Todo>
{
    new Todo(1, "Learn dotnet", DateTime.Now.AddDays(7), false),
    new Todo(2, "Build a REST API wit dotnet", DateTime.Now.AddDays(14), false),
    new Todo(3, "Learn abp.io", DateTime.Now.AddDays(-7), true),
    new Todo(4, "Learn abp.io and make todo app", DateTime.Now.AddDays(-7), true)
};

// Replace your separate routes with a single route that handles query parameters
app.MapGet("/todos", (bool? isCompleted, int? pageNumber, int pageSize = 10) =>
{
    var query = todos.AsQueryable();

    // Apply completion filter if provided
    if (isCompleted.HasValue)
    {
        query = query.Where(t => t.IsCompleted == isCompleted.Value);
    }

    // Apply pagination if provided
    if (pageNumber.HasValue && pageNumber.Value > 0)
    {
        query = query.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize);
    }

    return Results.Ok(query.ToList());
});
// Get all todos
app.MapGet("/todos/{id}", (int id) => Results.Ok(todos.FirstOrDefault(t => t.Id == id)));

// Create new todo with post method
app.MapPost("/todos", (Todo todo) =>
{
    todos.Add(todo);
    return Results.Created($"/todos/{todo.Id}", todo);
});

// update with put method
app.MapPut("/todos/{id}", (int id, Todo todo) =>
{
    var index = todos.FindIndex(t => t.Id == id);
    if (index == -1) return Results.NotFound();
    todos[index] = todo with { Id = id };
    return Results.Ok(todos[index]);
});

// update with patch method 
app.MapPatch("/todos/{id}", (int id, Todo todo) =>
{
    var index = todos.FindIndex(t => t.Id == id);
    if (index == -1) return Results.NotFound();
    todos[index] = todo with { Id = id };
    return Results.Ok(todos[index]);
});

// delete with delete method
app.MapDelete("/todos/{id}", (int id) =>
{
    var index = todos.FindIndex(t => t.Id == id);
    if (index == -1) return Results.NotFound();
    todos.RemoveAt(index);
    return Results.NoContent();
});

// Get all todos with isCompleted filter
app.MapGet("todos/isCompleted", (bool isCompleted) =>
{
    var filteredTodos = todos.Where(t => t.IsCompleted == isCompleted).ToList();
    return Results.Ok(filteredTodos);
});

//pagination
app.MapGet("todos/page/{pageNumber}", (int pageNumber) =>
{
    const int pageSize = 10;
    var paginatedTodos = todos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    return Results.Ok(paginatedTodos);
});

app.Run();


public record Todo(int Id, string Name, DateTime DueDate, bool IsCompleted)
{
    // Optional: Add validation or computed properties
    public bool IsPastDue => DateTime.Now > DueDate && !IsCompleted;
}