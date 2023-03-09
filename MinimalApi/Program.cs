using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//get all heroes
async Task<List<SuperHero>> GetAllHeroes(DataContext context) =>
    await context.SuperHeroes.ToListAsync();
//message
app.MapGet("/", () => "Welcome to Super Hero Db!");
//get all
app.MapGet("/superhero", async (DataContext context) =>
 await context.SuperHeroes.ToListAsync());
//find by id
app.MapGet("/superhero{id}", async (DataContext context, int id) =>
    await context.SuperHeroes.FindAsync(id) is SuperHero Hero ?
    Results.Ok(Hero) : 
    Results.NotFound("super hero not found :/")); ;
//create hero
app.MapPost("/createsuperhero", async (DataContext context, SuperHero hero,String heroname) =>
{
    var find = context.SuperHeroes.FindAsync(heroname);

    if (find != null) return Results.NotFound("Hero already exist");
    context.SuperHeroes.Add(hero);
    await context.SaveChangesAsync();
    return Results.Ok(await GetAllHeroes(context));
});

app.MapPut("/updatesuperhero/{id}", (DataContext context, SuperHero hero, int id) =>
{
    var findhero =  context.SuperHeroes.Find(id);
    if (findhero == null) return Results.NotFound("hero not found.  :/");

    findhero.FirstName = hero.FirstName;
    findhero.LastName = hero.LastName;
    findhero.HeroName = hero.HeroName;
     context.SaveChangesAsync();
    return Results.Ok(GetAllHeroes(context));
});

app.MapDelete("/deletesuperhero{id}", async (DataContext context, int id) =>
{
    var findhero = context.SuperHeroes.Find(id);
    if (findhero == null) return Results.NotFound("hero not found.  :/");

    context.SuperHeroes.Remove(findhero);
    await context.SaveChangesAsync();
    return Results.Ok(await GetAllHeroes(context));
});

app.Run();
