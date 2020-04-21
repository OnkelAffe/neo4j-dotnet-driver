using Neo4j.Driver;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains three examples of using this plugin that should get you going.
/// This example is based on the "Movies" example dataset by Neo4j (https://neo4j.com/developer/example-data/).
/// Comments are based on the Neo4j .NET Driver v4.1 documentation (https://github.com/neo4j/neo4j-dotnet-driver).
/// </summary>
public class Neo4JDriverSamples : MonoBehaviour
{
    void Start()
    {
        //SingleReturnTest();
        //SingleNodeLabelsTest();
        MultipleReturnsTest();
    }


    // ############
    // RETURN SINGLE LIST<STRING>
    // ############

    async void SingleReturnTest()
    {
        // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.        
        // The driver is thread-safe, while the session or the transaction is not thread-safe.
        IDriver driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "123456"));
        IAsyncSession session = driver.AsyncSession(o => o.WithDatabase("neo4j"));
        try
        {
            IResultCursor cursor = await session.RunAsync("MATCH (a:Person) RETURN a.name as name");
            // The recommended way to access these result records is to make use of methods provided by ResultCursorExtensions such as SingleAsync, 
            // ToListAsync, and ForEachAsync.
            List<string> people = await cursor.ToListAsync(record => record["name"].As<string>());
            await cursor.ConsumeAsync();

            Debug.Log(people.Count + " single returns");
        }
        finally
        {
            await session.CloseAsync();
        }
        await driver.CloseAsync();
    }


    // ############
    // RETURN SINGLE NODE
    // ############

    async void SingleNodeLabelsTest()
    {
        // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.        
        // The driver is thread-safe, while the session or the transaction is not thread-safe.
        IDriver driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "123456"));
        IAsyncSession session = driver.AsyncSession(o => o.WithDatabase("neo4j"));
        try
        {
            IResultCursor cursor = await session.RunAsync("MATCH (a {name: 'Joel Silver'}) RETURN a");
            // The recommended way to access these result records is to make use of methods provided by ResultCursorExtensions such as SingleAsync, 
            // ToListAsync, and ForEachAsync.
            INode person = await cursor.SingleAsync(record => record["a"].As<INode>());            
            await cursor.ConsumeAsync();

            foreach (var item in person.Labels)
            {
                Debug.Log(item);
            }
        }
        finally
        {
            await session.CloseAsync();
        }
        await driver.CloseAsync();
    }


    // ############
    // RETURN TWO LIST<STRING>
    // ############

    async void MultipleReturnsTest()
    {
        // Each IDriver instance maintains a pool of connections inside, as a result, it is recommended to only use one driver per application.        
        // The driver is thread-safe, while the session or the transaction is not thread-safe.
        IDriver driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "123456"));
        IAsyncSession session = driver.AsyncSession(o => o.WithDatabase("neo4j"));
        try
        {
            IResultCursor cursor = await session.RunAsync("MATCH (a:Person) RETURN a.name, a.born");
            // A record is accessible once it is received by the client. It is not needed for the whole result set to be received before it can be visited.
            // Each record can only be visited (a.k.a.consumed) once!
            // The recommended way to access these result records is to make use of methods provided by ResultCursorExtensions such as SingleAsync, 
            // ToListAsync, and ForEachAsync.
            List< DataHolder> people = await cursor.ToListAsync(record => new DataHolder(record["a.name"].As<string>(), record["a.born"].As<string>()));
            await cursor.ConsumeAsync();

            foreach (var item in people)
            {
                Debug.Log(item.ToString());
            }
        }
        finally
        {
            await session.CloseAsync();
        }
        await driver.CloseAsync();
    }

    // Helper Class
    public class DataHolder
    {
        public string A;
        public string B;

        public DataHolder (string a, string b)
        {
            A = a;
            B = b;
        }

        public string ToString ()
        {
            return A + " | " + B;
        }
    }
}
