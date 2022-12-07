// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using gRPCDemo.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System.IO.Compression;

var servicesCollection = new ServiceCollection();

servicesCollection.AddGrpc(o =>
{
    o.EnableDetailedErrors = true;
    o.ResponseCompressionLevel = CompressionLevel.Optimal;
});
servicesCollection.AddGrpcClient<DemoProtoService.DemoProtoServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:2000");
});

using(var provider = servicesCollection.BuildServiceProvider())
{
    var client = provider.GetRequiredService<DemoProtoService.DemoProtoServiceClient>();
    var response = client.ClientServer(new ClientServerRequest { Name = "Kanti" });

    Console.WriteLine(response.Message);

    var streamer = client.DoCLientStreaming();
    
    for(var i = 0; i < 10; i++)
    {
        streamer.RequestStream.WriteAsync(new DoCLientStreamingRequest { Number = i }).Wait();
    }

    streamer.RequestStream.CompleteAsync().Wait();

    Console.WriteLine(streamer.ResponseAsync.Result.Message);

    var responseStreamer = client.DoServerStreaming(new DoServerStreamingRequest { Message = "Hello" });
    while(responseStreamer.ResponseStream.MoveNext().Result)
    {
        Console.WriteLine($"{DateTime.UtcNow.ToLongTimeString()}: {responseStreamer.ResponseStream.Current.Message}");
    }

    Console.WriteLine("Completed");

    var streamer2 = client.DODuplexStreaming();
    
    for(int i = 0; i <= 10; i++)
    {
        Console.WriteLine($"Sending {i}");
        streamer2.RequestStream.WriteAsync(new DODuplexStreamingRequest { Number = i }).Wait();
    }

    streamer2.RequestStream.CompleteAsync().Wait();

    while (streamer2.ResponseStream.MoveNext().Result)
    {
        Console.WriteLine($"Received: {streamer2.ResponseStream.Current.Number}");
    }

    Console.WriteLine("Duplex completed");
}