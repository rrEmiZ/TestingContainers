using Dapper;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit.Abstractions;

namespace TestingContainers;

public class DatabaseContainerPerTest(ITestOutputHelper output) : IAsyncLifetime
{
    private readonly MsSqlContainer container =
        new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server")
        .WithPassword("P@ssw0rd")
        .Build();

    private string connectionString = string.Empty;

    [Fact]
    public async Task Database_Can_Run_Query()
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        const int expected = 1;
        var actual = await connection.QueryFirstAsync<int>("SELECT 1");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Database_Can_Select_DateTime()
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        var actual = await connection.QueryFirstAsync<DateTime>("SELECT GETDATE()");
        Assert.IsType<DateTime>(actual);
    }


    [Fact]
    public async Task Database_Dummy_Query_Call()
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        var query = """
                WITH Numbers AS (
                    SELECT 1 AS Number
                    UNION ALL
                    SELECT Number + 1
                    FROM Numbers
                    WHERE Number < 10
                )
                SELECT
                    Number,
                    GETDATE() AS CurrentDate,
                    NEWID() AS UniqueIdentifier,
                    ROW_NUMBER() OVER (ORDER BY Number) AS RowNum
                FROM Numbers
                OPTION (MAXRECURSION 10);
            """;


        var results = await connection.QueryAsync<(int Number, DateTime CurrentDate, Guid UniqueIdentifier, int RowNum)>(query);

        Assert.Equal(10, results.Count());
    }



    public async Task InitializeAsync()
    {
        await container.StartAsync();
        connectionString = container.GetConnectionString();
        output.WriteLine(container.Id);
    }

    public Task DisposeAsync() => container.StopAsync();
}