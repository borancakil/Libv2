# ADR-002: Use of Entity Framework Core for Data Access

**Date**: 2025-08-22

**Status**: Accepted

## Context

We need a reliable and efficient way to access and manage our application's data in a Microsoft SQL Server database. The chosen data access technology should integrate well with our .NET backend and provide a good level of abstraction over the underlying database.

## Decision

We have chosen to use Entity Framework Core (EF Core) as our primary data access technology. EF Core is a modern, object-relational mapper (O/RM) for .NET that simplifies database interactions and allows us to work with our data using C# objects.

## Consequences

### Advantages

*   **Productivity**: EF Core simplifies data access and reduces the amount of boilerplate code we need to write.
*   **Performance**: EF Core is highly performant and includes features like query caching and change tracking to optimize database interactions.
*   **Portability**: EF Core supports multiple database providers, which gives us the flexibility to switch to a different database in the future if needed.
*   **Migrations**: EF Core's migration feature makes it easy to manage and evolve our database schema over time.

### Disadvantages

*   **Learning Curve**: There is a learning curve associated with EF Core, especially for developers who are not familiar with O/RMs.
*   **Complexity**: For very complex queries, it may be necessary to drop down to raw SQL, which can add complexity to the codebase.
