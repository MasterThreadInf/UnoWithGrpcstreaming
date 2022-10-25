using Google.Protobuf.WellKnownTypes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using GrpcSamples;
using Grpc.Net.Client;
using Grpc.Core;
//using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Windows.Media.Protection.PlayReady;
using System.ServiceModel.Channels;
using Grpc.Net.Client.Web;
using System.Net;
using static Grpc.Core.Metadata;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Net.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ChatTrialSolution
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static string ResponseMessageFromserver;
        public MainPage()
        {
            this.InitializeComponent();
        
        }
      

        public async void SendMsg_Click(object sender, EventArgs e)
        {
            var factory = new StaticResolverFactory(addr => new[]
{
                new BalancerAddress("localhost", 44366),
                new BalancerAddress("localhost", 81)
            });
            var services = new ServiceCollection();
            services.AddSingleton<ResolverFactory>(factory);
            //GrpcClientFactory.AllowUnencryptedHttp2 = true;
            var httpHandler = new HttpClientHandler();
            //httpHandler.ServerCertificateCustomValidationCallback =
            //HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
          
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, httpHandler));

            //"https://localhost:44366"
            //var options = new GrpcChannelOptions() { HttpClient= httpClient };
            var channel = GrpcChannel.ForAddress("static:///my-example-host", new GrpcChannelOptions { HttpClient = httpClient,
                Credentials = ChannelCredentials.Insecure,
                ServiceProvider = services.BuildServiceProvider(),
                ServiceConfig = new ServiceConfig { LoadBalancingConfigs = { new RoundRobinConfig() } }
            });
            //var channel = GrpcChannel.ForAddress("https://localhost:44366", new GrpcChannelOptions { HttpHandler = new GrpcWebHandler(new HttpClientHandler())});
            try
            {
                var client = new FooService.FooServiceClient(channel);
               
                //await RunSyncUnaryRpc(client, MsgValue.Text, Msgread).ConfigureAwait(false);//Works
               // await RunAsyncUnaryRpc(client, MsgValue.Text, Msgread).ConfigureAwait(false);//Works
                //await RunServerStreamingRpc(client, MsgValue.Text, Msgread).ConfigureAwait(false);//works
                await RunClientStreamingRpc(client, MsgValue.Text, Msgread).ConfigureAwait(false); // Doesnt work 
                // await RunBidirectionalRpc(client, MsgValue.Text, Msgread).ConfigureAwait(false);// Doesnt work 

                //MsgValue.Text = $"{Message }";
                //Msgread.Text = ResponseMessageFromserver;
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                await channel.ShutdownAsync();
            }

            
        }
        /// <summary>
        /// A delegating handler that changes the request HTTP version to HTTP/3.
        /// </summary>
        public class Http3Handler : DelegatingHandler
        {
            public Http3Handler() { }
            public Http3Handler(HttpMessageHandler innerHandler) : base(innerHandler) { }
            request.Properties[WebAssemblyEnableStreamingResponseKey] = true; 
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Version =HttpVersion.Version20;// new win11 have http3 so it may work there (Windows 11 Build 22000 or later OR Windows Server 2022.And TLS 1.3 or later connection.)
                request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

                return base.SendAsync(request, cancellationToken);
            }
        }
        private static async Task RunSyncUnaryRpc(FooService.FooServiceClient client, string textMsg, Microsoft.UI.Xaml.Controls.TextBlock readTxt)
        {
            Console.WriteLine("Starting unary RPC example...\n", ConsoleColor.Blue);

            // Synchronous unary RPC
            var fooSynchronousRequest = new FooRequest { Message = string.Format("{0} Sync request", textMsg) };
            var fooSynchronousResponse = client.GetFoo(fooSynchronousRequest);


            Console.WriteLine($"\t{fooSynchronousResponse.Message}");
        
            readTxt.Text = fooSynchronousResponse.Message;

            Console.WriteLine("***********Finished unary RPC example*************");

        }
        private static async Task RunAsyncUnaryRpc(FooService.FooServiceClient client,string textMsg, Microsoft.UI.Xaml.Controls.TextBlock readTxt)
        {
            Console.WriteLine("Starting unary RPC example...\n", ConsoleColor.Blue);

            // Synchronous unary RPC
            //var fooSynchronousRequest = new FooRequest { Message = string.Format ("{0} Sync request",textMsg) };
            //var fooSynchronousResponse = client.GetFoo(fooSynchronousRequest);

            //// Asynchronous unary RPC
            var fooAsynchronousRequest = new FooRequest { Message = string.Format("{0} asynchronous request", textMsg) };
            var fooAsynchronousResponse = await client.GetFooAsync(fooAsynchronousRequest);

            //Console.WriteLine($"\t{fooSynchronousResponse.Message}");
            Console.WriteLine($"\t{fooAsynchronousResponse.Message}\n");
            readTxt.Text = fooAsynchronousResponse.Message;

            Console.WriteLine("***********Finished unary RPC example*************");
     
        }
        private static async Task RunServerStreamingRpc(FooService.FooServiceClient client, string textMsg,Microsoft.UI.Xaml.Controls.TextBlock  readTxt)
        {
            Console.WriteLine("Starting server streaming RPC example...\n", ConsoleColor.Blue);

            Console.WriteLine("You will be requested for an amount of messages that should be streamed to client.");
            Console.WriteLine("gRPC Server will then stream this amount of messages before it will finish the request.");

            Console.WriteLine("Amount of messages:");
            var result = textMsg;

            var fooRequest = new FooServerStreamingRequest { Message = textMsg, MessageCount = 1 };
            var serverStreamingCall = client.GetFoos(fooRequest);

            Console.WriteLine($"\n\tgRPC Server responses:");

            await foreach (var response in serverStreamingCall.ResponseStream.ReadAllAsync())
            {
                //Console.WriteLine($"\t> {response.Message}");
                readTxt.Text += response.Message.ToString();
            }

            Console.WriteLine("\n***********Finished server streaming RPC example**********", ConsoleColor.Blue);
        }
        private static async Task RunClientStreamingRpc(FooService.FooServiceClient client, string textMsg,Microsoft.UI.Xaml.Controls.TextBlock readTxt)
        {
            var clientStreamingCall =  client.SendFoos(new CallOptions(deadline: DateTime.UtcNow.AddSeconds(5)));

            while (true)
            {
                var result = textMsg;

                if (string.IsNullOrEmpty(result))
                {
                    await clientStreamingCall.RequestStream.CompleteAsync().ConfigureAwait(false);
                    break;
                }

                var fooRequest = new FooRequest { Message = result };
          
                //var x= GrpcServer.DefaultFooService.SendFoos(fooRequest,null);
                await clientStreamingCall.RequestStream.WriteAsync(fooRequest).ConfigureAwait(false);
            }

            var response = await clientStreamingCall.ResponseAsync.ConfigureAwait(false);
            Console.WriteLine($"\n\t gRPC Server response:\n\t> {response.Message}\n");
            readTxt.Text = response.Message;
        }
      
        private static async Task RunBidirectionalRpc(FooService.FooServiceClient client, string textMsg, Microsoft.UI.Xaml.Controls.TextBlock readTxt)
        {
            Console.WriteLine("Starting bidirectional streaming RPC example......\n");
            //Console.WriteLine("You will be requested for messages that should be streamed to server.");
            //Console.WriteLine("Each message will be streamed to server until request client requests finishes.");
            //Console.WriteLine("To finish the request just leave the message empty.");

            var receivedMessages = new List<string>();
            var bidirectionalCall = client.SendAndGetFoos(deadline: DateTime.UtcNow.AddSeconds(5));

            var readTask = Task.Run(async () =>
            {
                await foreach (var response in bidirectionalCall.ResponseStream.ReadAllAsync())
                {
                    receivedMessages.Add(response.Message);
                }
            });

            while (true)
            {
                var result = textMsg;

                if (string.IsNullOrEmpty(result))
                {
                    break;
                }

                await bidirectionalCall.RequestStream.WriteAsync(new FooRequest { Message = result });
            }

            await bidirectionalCall.RequestStream.CompleteAsync();

            Console.WriteLine($"\n\tgRPC Server responses:");

            foreach (var receivedMessage in receivedMessages)
            {
                Console.WriteLine($"\t> {receivedMessage}");
                readTxt.Text += String.Format("-{0}", receivedMessage);
            }
            Console.WriteLine("\n*****************************Finished!*****************************************\n", ConsoleColor.Blue);
        }


        public Streams[] StreamingList => new[]
        {
        Streams.Unary,
        Streams.ClientSide,
        Streams.ServerSide,
        Streams.Bidirection,

    };
        public enum Streams
        {
            Unary,
            ClientSide,
            ServerSide,
            Bidirection,
        }
        //public async void Streaming_SelectionChanged(object sender, SelectionChangedEventArgs  e)
        //{

        //}

    }
}
