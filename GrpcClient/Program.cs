using Grpc.Net.Client;
using GrpcServer;
using Grpc.Core;

using var channel = GrpcChannel.ForAddress("http://localhost:5220");
var client = new Greeter.GreeterClient(channel);

Console.WriteLine("=== gRPC Demo: Unary and Server Streaming ===\n");

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
