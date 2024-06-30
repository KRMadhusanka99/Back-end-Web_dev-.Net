using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
// app.MapGet("/", () => "Hello World!");

// Add middleware to the pipeline
app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "todos/$1"));

//add Custom Middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{context.Request.Method}] {context.Request.Path} {DateTime.UtcNow}] Started");
    await next(context);
    Console.WriteLine($"[{context.Request.Method}] {context.Request.Path} {DateTime.UtcNow}] Finished");
});

var todos = new List<Todo>();

// Post
app.MapPost("/todos", (Todo task) =>
{
    todos.Add(task);
    return TypedResults.Created("/todos/{id}", task);
});

// Get all
app.MapGet("/todos", () => todos);  

// Get by id
app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => {
    var targetTodo = todos.SingleOrDefault(t => t.Id == id);
    return targetTodo is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(targetTodo);
});

// Delete by id
app.MapDelete("/todos/{id}", (int id)=>{
    todos.RemoveAll(t => t.Id == id);
    return Results.NoContent();
}); 

app.Run();

public record Todo(int Id, string Name, DateTime DueDate, bool IsComplete);