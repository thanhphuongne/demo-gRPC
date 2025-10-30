using Grpc.Net.Client;
using GrpcServer;
using Grpc.Core;

using var channel = GrpcChannel.ForAddress("http://localhost:5220");
var client = new Greeter.GreeterClient(channel);

Console.WriteLine("=== gRPC Demo: All Communication Patterns ===\n");

try
{
    // 1. Unary RPC
    Console.WriteLine("1. Unary RPC Demo");
    Console.Write("Enter your name (try 'error' for error handling demo): ");
    var name = Console.ReadLine();

    if (string.IsNullOrEmpty(name))
    {
        name = "World";
    }

    try
    {
        var reply = await client.SayHelloAsync(new HelloRequest { Name = name });
        Console.WriteLine($"Unary Response: {reply.Message}\n");
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
    {
        Console.WriteLine($"Validation Error: {ex.Status.Detail}\n");
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Internal)
    {
        Console.WriteLine($"Server Error: {ex.Status.Detail}\n");
    }

    // 2. Server Streaming RPC
    Console.WriteLine("2. Server Streaming RPC Demo");
    Console.WriteLine($"Server streaming greetings for '{name}':");

    using (var call = client.ServerStreamingHello(new HelloRequest { Name = name }))
    {
        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"  {response.Message}");
        }
    }
    Console.WriteLine();

    // 3. Client Streaming RPC
    Console.WriteLine("3. Client Streaming RPC Demo");
    Console.WriteLine("Enter multiple names (press Enter after each, empty line to finish):");

    using (var call = client.ClientStreamingHello())
    {
        string input;
        while (!string.IsNullOrEmpty(input = Console.ReadLine() ?? ""))
        {
            await call.RequestStream.WriteAsync(new HelloRequest { Name = input });
        }
        await call.RequestStream.CompleteAsync();

        var countReply = await call;
        Console.WriteLine($"Client Streaming Response: {countReply.Summary}\n");
    }

    // 4. Bidirectional Streaming RPC
    Console.WriteLine("4. Bidirectional Streaming RPC Demo");
    Console.WriteLine("Enter names for bidirectional chat (empty line to finish):");

    using (var call = client.BidirectionalHello())
    {
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"Server: {response.Message}");
            }
        });

        string input;
        while (!string.IsNullOrEmpty(input = Console.ReadLine() ?? ""))
        {
            await call.RequestStream.WriteAsync(new HelloRequest { Name = input });
        }
        await call.RequestStream.CompleteAsync();

        await readTask;
    }

    Console.WriteLine("\n=== Demo Complete ===");
}
catch (RpcException ex)
{
    Console.WriteLine($"gRPC Error: {ex.StatusCode} - {ex.Status.Detail}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
