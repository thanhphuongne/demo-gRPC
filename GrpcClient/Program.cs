using Grpc.Net.Client;
using GrpcServer;

using var channel = GrpcChannel.ForAddress("http://localhost:5220");
var client = new Greeter.GreeterClient(channel);

Console.Write("Enter your name: ");
var name = Console.ReadLine();

if (string.IsNullOrEmpty(name))
{
    name = "World";
}

var reply = await client.SayHelloAsync(new HelloRequest { Name = name });
Console.WriteLine($"Greeting: {reply.Message}");

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
