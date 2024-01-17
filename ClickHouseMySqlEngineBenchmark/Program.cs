﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using MySql.Data.MySqlClient;

Console.WriteLine("Hello, World!");

var summary = BenchmarkRunner.Run<QueryService>();

public class QueryService
{
    [Params("SumGroupByOrderType", "SumGroupByProductId")]
    public string QueryTarget { get; set; }
    Dictionary<string, string> QueryStrDictionary = new Dictionary<string, string>()
    {
        {"SumGroupByOrderType","select order_type,sum(amount) from test.orders group by order_type;"},
        {"SumGroupByProductId","select product_id,sum(amount) from test.orders group by product_id;"}
    };
    
    Dictionary<string, string> QueryStrClickHouseDictionary = new Dictionary<string, string>()
    {
        {"SumGroupByOrderType","select order_type,sum(toDecimal64(amount, 6)) from mysql.orders group by order_type;"},
        {"SumGroupByProductId","select product_id,sum(toDecimal64(amount, 6)) from mysql.orders group by product_id;"}
    };

    [Benchmark]
    public async Task QueryMysql()
    {
        const string connectionString =
            "Server=localhost;Port=3306;Database=test;Username=root;Password=pass.123;Allow User Variables=true;default command timeout=200";

        await using var connection = new MySqlConnection(connectionString);
        connection.Open();var cmd = new MySqlCommand(QueryStrDictionary[QueryTarget], connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (reader.Read()) Console.WriteLine($"{reader[reader.GetName(0)]} - {reader[reader.GetName(1)]}");
    }

    [Benchmark]
    public async Task QueryClickHouseTable()
    {
        var connectionString = "Compression=True;Timeout=10000;Host=localhost;Port=8123;Database=test;Username=yowko;Password=pass.123";

        await using ClickHouseConnection connection = new ClickHouseConnection(connectionString);
        await using var reader = await connection.ExecuteReaderAsync(QueryStrDictionary[QueryTarget]);
        while (reader.Read()) Console.WriteLine($"{reader[reader.GetName(0)]} - {reader[reader.GetName(1)]}");
    }
    
    [Benchmark]
    public async Task QueryClickHouseDatabase()
    {
        var connectionString = "Compression=True;Timeout=10000;Host=localhost;Port=8123;Database=test;Username=yowko;Password=pass.123";

        await using ClickHouseConnection connection = new ClickHouseConnection(connectionString);
        await using var reader = await connection.ExecuteReaderAsync(QueryStrClickHouseDictionary[QueryTarget]);
        while (reader.Read()) Console.WriteLine($"{reader[reader.GetName(0)]} - {reader[reader.GetName(1)]}");
    }
}