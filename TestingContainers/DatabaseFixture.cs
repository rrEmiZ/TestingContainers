using Testcontainers.MsSql;

namespace TestingContainers;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer container = 
        new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server")
        .WithPassword("P@ssw0rd")
            .Build();
    
    public string ConnectionString => container.GetConnectionString();
    public string ContainerId => $"{container.Id}";

    public Task InitializeAsync() 
        => container.StartAsync();

    public Task DisposeAsync() 
        => container.DisposeAsync().AsTask();
}