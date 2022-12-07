using Grpc.Core;
using gRPCDemo.Entities;
using gRPCDemo.Service;
using System.Text;

namespace gRPCDemo.Service.Services
{
    public class DemoService : DemoProtoService.DemoProtoServiceBase
    {
        public override Task<ClientServerReply> ClientServer(ClientServerRequest request, ServerCallContext context)
        {
            Console.WriteLine(request.Name);
            return Task.FromResult<ClientServerReply>(new ClientServerReply { Message = $"Hello {request.Name}" });
        }

        public override async Task<DoCLientStreamingResponse> DoCLientStreaming(IAsyncStreamReader<DoCLientStreamingRequest> requestStream, ServerCallContext context)
        {
            var sb = new StringBuilder();

            while(await requestStream.MoveNext())
            {
                var number = requestStream.Current.Number;
                Console.WriteLine(number);
                sb.Append(number.ToString());
            }

            Console.WriteLine("Completed");
            return new DoCLientStreamingResponse { Message = sb.ToString() };
        }

        public override Task DoServerStreaming(DoServerStreamingRequest request, IServerStreamWriter<DoServerStreamingResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine(request.Message);

            responseStream.WriteAsync(new DoServerStreamingResponse { Message = "Started"}).Wait();
            Thread.Sleep(1000);
            responseStream.WriteAsync(new DoServerStreamingResponse { Message = "Processing" }).Wait();
            Thread.Sleep(2000);
            responseStream.WriteAsync(new DoServerStreamingResponse { Message = "Completed" }).Wait();

            return Task.CompletedTask;
        }

        public override async Task DODuplexStreaming(IAsyncStreamReader<DODuplexStreamingRequest> requestStream, IServerStreamWriter<DODuplexStreamingResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Console.WriteLine(requestStream.Current.Number);
                responseStream.WriteAsync(new DODuplexStreamingResponse { Number= requestStream.Current.Number }).Wait();
            }
        }
    }
}