# Forum Web API

## Task Code: FORUM

### Overview

This project implements a Web API application using ASP.NET Core, designed according to the specified non-functional and architectural requirements. The application supports a user role system, manages data in a relational database, and follows best practices in code organization and error handling.

### Technologies Used

- **ASP.NET Core**: Framework for building the Web API.
- **Entity Framework Core**: ORM for accessing and managing data in a SQL Server Express database.
- **SQL Server Express**: Database management system for storing application data.
- **JSON Configuration**: For reading configuration settings from JSON files.
- **Logging**: Integrated logging using ASP.NET Core logging features.

### Project Structure

The solution is organized into three projects, representing the three layers of the application architecture:

1. **WebApp.WebApi**: Contains the presentation layer with Web API controllers.
2. **WebApp.BusinessLogic**: Contains business logic and application services.
3. **WebApp.DataAccess**: Contains the data access layer with repository implementations.

### Features Implemented

- **User Role System**: Supports multiple roles with different capabilities (e.g., Admin, Moderator, User).
- **CRUD Operations**: Implemented using repositories that provide methods for managing data entities.
- **Data Validation**: Validation logic added to ensure correct input is passed to services and repositories.
- **Error Handling**: 
  - Meaningful status codes returned from controller actions based on exceptions.
  - Internal Server Error status code returned for unexpected exceptions.
  - Logging of errors, warnings, and trace messages.
- **Pagination**: All endpoints returning lists of data entities are paginated to enhance performance.
- **Entity Framework Migrations**: Used to manage and evolve the database schema smoothly.
- **Code Quality**: 
  - Complied with .NET source code analysis.
  - Adhered to naming conventions and best practices.
  - Included XML documentation comments for public classes and methods.