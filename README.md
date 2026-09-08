# KiteSocial.API

A kitesurfing social network backend API built with ASP.NET Core (.NET 10).

## Architecture & Structure
- **Controllers/**: API endpoint controllers (`/api/[controller]`)
- **Models/Entities/**: Domain models and database entities
- **Data/**: EF Core DbContext, entity configurations, migrations
- **DTOs/**: Request and Response Data Transfer Objects
- **Services/**: Business logic layer and interfaces

## Tech Stack
- **Framework**: .NET 10 (`net10.0`)
- **Architecture**: Clean Architecture / N-tier with Controllers
- **API Documentation**: OpenAPI / Swagger

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run locally
```bash
dotnet restore
dotnet run
```
Test health endpoint:
```bash
curl http://localhost:5067/api/health
```
