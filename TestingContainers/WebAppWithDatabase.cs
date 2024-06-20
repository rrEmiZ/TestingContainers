using System.Net.Http.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;

namespace TestingContainers;

public class WebAppWithDatabase(DatabaseFixture fixture) 
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    [Fact]
    public async Task Get_Information_From_Database_Endpoint()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => {
               
                host.UseSetting(
                    "ConnectionStrings:database", 
                    fixture.ConnectionString
                );
            });

        var client = factory.CreateClient();
        var actual = await client.GetFromJsonAsync<Car>("/database?make=Honda");
        
        Assert.Equal(expected: "Civic", actual?.Model);
    }

    public async Task InitializeAsync()
    {
        var connection = new SqlConnection(fixture.ConnectionString);

        await connection.ExecuteAsync(
            """
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') AND name = 'Cars')
                BEGIN
                    CREATE TABLE Cars (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        make VARCHAR(255),
                        model VARCHAR(255),
                        year INT
                    );

                    INSERT INTO Cars (make, model, year) VALUES
                    ('Audi', 'A4', 2024),
                    ('Honda', 'Civic', 2024),
                    ('Ford', 'Focus', 2024);
                END;
            """
        );
    }

    public Task DisposeAsync() => Task.CompletedTask;
}