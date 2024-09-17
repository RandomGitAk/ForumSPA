# Web Application: Web API

In this task, you must design and develop a web application according to the requirements that are specified in the task description.

The brief description of the technologies to use in this task is:
* The Web API application must be built on top of the ASP.NET Core framework.
* The Web API application must provide a RESTful API with meaningful status codes and URIs.
* The data store must be a relational database management system such as SQL Server Express.
* You must use Entity Framework Core to access application data.

Depending on the requirements, the task may include requirements for developing a client application. The client application code must be added to a *separate repository*.


## Task Description

This document describes only the [non-functional requirements](https://en.wikipedia.org/wiki/Non-functional_requirement) for the application being developed. You must either develop [functional requirements](https://en.wikipedia.org/wiki/Functional_requirement) by yourself, depending on the topic received, or receive them from another source (a mentor, lecturer, or learning path).

Before starting the development, put the task code you have received into the [README.md](README.md) file:

TASK CODE: FORUM.


### Target Framework

The Web API application and .NET libraries in this solution must target [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

When creating a C# project, ensure that the [target framework moniker](https://learn.microsoft.com/en-us/dotnet/standard/frameworks) is set to `net6.0` in a project file:

```xml
<TargetFramework>net6.0</TargetFramework>
```

If you're having trouble developing .NET 6 applications, make sure you have the .NET SDK 6 installed by running the [dotnet command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet) in the console:

```cmd
C:\>dotnet --list-sdks
6.0.408 [C:\Program Files\dotnet\sdk]
```

If you do not have the .NET SDK 6 installed, you can download the installer from the [Download .NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) page.


### Git

* Do all your development on the *main* branch.
* [Do commit early and often](https://sethrobertson.github.io/GitBestPractices/).
* [Do make useful commit messages](https://sethrobertson.github.io/GitBestPractices/).
* You are allowed to create your own branches, but at the end, the code must be merged into the *main* branch.
* Do not create merge requests unless asked to do so by staff.
* The [.gitignore](.gitignore) file is added to this Git repository, which configures Git to ignore files created by the Visual Studio and compilers.


### Architecture

* The application must be implemented as an ASP.NET Core [Web API application](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis).
* The solution has a [3-tier architecture](https://en.wikipedia.org/wiki/Multitier_architecture):
    * The presentation tier: if the requirements of the task involve the development of a client application, place the client application code in a separate repository.
    * The logic (application) tier — the application server you must develop for this task.
    * The data tier is the relational database management system for storing application data.
* The application server must be implemented as a [three-layered architecture](https://www.hanselman.com/blog/a-reminder-on-threemulti-tierlayer-architecturedesign-brought-to-you-by-my-late-night-frustrations). The code for each layer must be organized as a separate C# project.
    * The presentation layer in this application is a set of Web API controllers that are in the [WebApp.WebApi](WebApp.WebApi).
    * The business logic layer: put the code that is related to this layer into the [WebApp.BusinessLogic](WebApp.BusinessLogic) project.
    * The data access layer: put the code that is related to this layer into the [WebApp.DataAccess](WebApp.DataAccess) project.
* The choice of database management system for the data tier depends on the platform on which the development and deployment of the application will be carried out.
    * If you are a Windows user, consider using [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16). You can enable this feature through the [Visual Studio Installer](https://visualstudio.microsoft.com/downloads) (see the details on the page).
    * You may also use [SQL Server Express 2022](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2022).
    * If you are a Mac or Linux user, consider using [PostgreSQL](https://www.postgresql.org/) or [SQLite](https://sqlite.org/).


### Application Requirements

* All C# projects must be added to the [WebApp](WebApp.sln) solution.
* An application must support a user role system that allows a user to have different capabilities depending on the role the user belongs to. Add at least three different roles.
* The application must store its data in a relational database.
* Add data validation to your Web API controllers to avoid situations when incorrect input is passed to application services and repositories.
* The application must have the right approach implemented for handling application errors:
    * The controller actions must handle exceptions thrown by application services and repositories.
    * The controller action must return a [meaningful status code](https://en.wikipedia.org/wiki/List_of_HTTP_status_codes) when the expected exception is thrown.
    * The controller action must return an "Internal Server Error" status code if an unexpected exception is thrown.
    * The controller action must log events (errors, warnings, and trace messages) using the [ASP.NET Core logging features](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging).
* The application must read its configuration settings from the [JSON settings files](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) using the [JSON configuration provider](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration#json-configuration-provider).
* All the endpoints of an API that return a list of data entities must be paginated. You have to decide how to implement pagination for your API.
* The data access layer is a set of repositories that provide CRUD operations for managing data entities.
    * A [repository](https://www.martinfowler.com/eaaCatalog/repository.html) is a class that provides CRUD operations for finding and managing the data entities it is responsible for.
    * The repository methods for each repository must be declared in the appropriate [interface](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface), which the repository class must implement.
    * Use the [Entity Framework Core](https://learn.microsoft.com/en-us/ef) object-relational mapper to access the database tables; data access should be designed with a *code-first* approach.
    * During application development, use [EF migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations) to smoothly evolve your database.
* When the application is ready, review the application's performance and eliminate all performance issues.


### Code

* The settings in the [.editorconfig](.editorconfig) file are used to configure code style rules. Avoid changing these settings.
    * The [csharp_style_namespace_declarations](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0160-ide0161) parameter is set to `file_scoped`. You may change this setting to `block_scoped` if you prefer having block-scoped namespaces instead of file-scoped.
* The source code must be well organized so that it can be [easily reused](https://en.wikipedia.org/wiki/Code_reuse) by other applications.
* The classes and class methods must have [meaningful names](https://pspdfkit.com/blog/2018/naming-classes-why-it-matters-how-to-do-it-well) that are easy to understand.
* The public classes and interfaces, public constructors, and public methods must be documented; that is, they must have [XML documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/).
* The source code must comply with the [Framework design guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines).
* All C# project files in this solution must have the built-in [.NET source code analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) feature enabled. See the [Code Analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview) documentation section for more details.

```xml
<EnableNETAnalyzers>true</EnableNETAnalyzers>
<AnalysisMode>AllEnabledByDefault</AnalysisMode>
<AnalysisLevel>latest</AnalysisLevel>
<CodeAnalysisTreatWarningsAsErrors>false</CodeAnalysisTreatWarningsAsErrors>
```

* All C# project files in this solution must have the [StyleCop.Analyzers NuGet package](https://www.nuget.org/packages/StyleCop.Analyzers) installed.

```xml
<ItemGroup>
    <AdditionalFiles Include="..\stylecop.json" Link="Properties\stylecop.json" />
</ItemGroup>
<ItemGroup>
    <PackageReference Include="StyleCop.Analyzers" Version="1.1.118">
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        <PrivateAssets>all</PrivateAssets>
    </PackageReference>
</ItemGroup>
```

* If you use the Visual Studio, we recommend you install the [SonarLint for Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=SonarSource.SonarLintforVisualStudio2022) extension. The SonarLint extension detects all Sonar issues, and this would reduce the number of issues in the Quality phase when submitting the task in the AutoCode.