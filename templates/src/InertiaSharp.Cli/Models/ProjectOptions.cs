namespace InertiaSharp.Cli.Models;

public enum ApiStyle  { Mvc, MinimalApi }
public enum Database  { Sqlite, PostgreSQL, SqlServer }
public enum Frontend  { Vue, React, Svelte }

public sealed class ProjectOptions
{
    public required string  ProjectName     { get; init; }
    public required string  OutputDirectory { get; init; }
    public required ApiStyle ApiStyle       { get; init; }
    public required Database Database       { get; init; }
    public required Frontend Frontend       { get; init; }
    public required bool    IncludeAuth     { get; init; }

    public string Namespace => ProjectName.Replace("-", "_").Replace(" ", "_");

    public string DbConnectionString => Database switch
    {
        Database.Sqlite     => $"Data Source={ProjectName.ToLower()}.db",
        Database.PostgreSQL => $"Host=localhost;Database={ProjectName.ToLower()};Username=postgres;Password=postgres",
        Database.SqlServer  => $"Server=(localdb)\\\\mssqllocaldb;Database={ProjectName};Trusted_Connection=True;",
        _                   => throw new ArgumentOutOfRangeException()
    };

    public string VitePort => Frontend switch
    {
        Frontend.Vue    => "5173",
        Frontend.React  => "5174",
        Frontend.Svelte => "5175",
        _               => "5173"
    };
}
