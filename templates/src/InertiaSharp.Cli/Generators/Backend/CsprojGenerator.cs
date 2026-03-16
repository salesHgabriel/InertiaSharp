using System.Text.Json;
using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Backend;

public static class CsprojGenerator
{
  private static readonly HttpClient Http = new();

  public static string Generate(ProjectOptions opts)
  {
      string lastVersionInertiaSharp = GetLatestVersion().Result ?? throw new Exception("No version available");
      
        var dbPackage = opts.Database switch
        {
            Database.Sqlite     => """<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />""",
            Database.PostgreSQL => """<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />""",
            Database.SqlServer  => """<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0-*" />""",
            _                   => throw new ArgumentOutOfRangeException()
        };

        var authPackage = opts.IncludeAuth
            ? """<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.3" />"""
            : string.Empty;

        var vitePort = opts.VitePort;

        return $"""
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <RootNamespace>{opts.Namespace}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="InertiaSharp" Version="{lastVersionInertiaSharp}" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    {dbPackage}
    {authPackage}
  </ItemGroup>

  <ItemGroup>
    <Folder Include="Migrations\" />
    <Folder Include="wwwroot\" />
  </ItemGroup>

  <ItemGroup>
    <_ContentIncludedByDefault Remove="ClientApp\components.json" />
    <_ContentIncludedByDefault Remove="ClientApp\package-lock.json" />
    <_ContentIncludedByDefault Remove="ClientApp\package.json" />
    <_ContentIncludedByDefault Remove="ClientApp\tsconfig.json" />
  </ItemGroup>

  <Target Name="PublishFrontend" AfterTargets="Build" Condition="'$(Configuration)' == 'Release'">
    <Exec WorkingDirectory="ClientApp" Command="npm ci" />
    <Exec WorkingDirectory="ClientApp" Command="npm run build" />
  </Target>

</Project>
""";
    }
  
  public static async Task<string?> GetLatestVersion(string package = "inertiasharp")
  {
    var url = $"https://api.nuget.org/v3-flatcontainer/{package.ToLower()}/index.json";

    var json = await Http.GetStringAsync(url);

    using var doc = JsonDocument.Parse(json);
    var versions = doc.RootElement.GetProperty("versions");

    return versions[versions.GetArrayLength() - 1].GetString();
  }
    
}
