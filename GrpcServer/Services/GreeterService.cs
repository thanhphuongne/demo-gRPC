using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Unary call received for: {request.Name}");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Name cannot be empty"));
        }

        if (request.Name.ToLower() == "error")
        {
            throw new RpcException(new Status(StatusCode.Internal, "Simulated server error"));
        }

        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}!"
        });
    }

    public override async Task ServerStreamingHello(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation($"Server streaming call received for: {request.Name}");

        var greetings = new[]
        {
            $"Hello {request.Name}!",
            $"Nice to meet you, {request.Name}!",
            $"How are you doing today, {request.Name}?",
            $"Goodbye for now, {request.Name}!"
        };

        foreach (var greeting in greetings)
        {
            await responseStream.WriteAsync(new HelloReply { Message = greeting });
            await Task.Delay(1000); // Simulate processing time
        }
    }
}
