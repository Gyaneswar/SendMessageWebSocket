using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendMessageWebSocket;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {

        string output = JsonConvert.SerializeObject(input);
        Console.WriteLine(output);
        Console.WriteLine(context);

        var connectionId = input.RequestContext.ConnectionId;
        Console.WriteLine(connectionId);
        Console.WriteLine("**********************************");
        var message = JsonConvert.DeserializeObject<WebSocketCallMsgBody>(input.Body).message;
        Console.WriteLine(message);
        var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(message));

        var domainName = input.RequestContext.DomainName;
        var stage = input.RequestContext.Stage;
        var endpoint = $"https://{domainName}/{stage}";
        Console.WriteLine("**********************************");
        Console.WriteLine(endpoint);

        var apiClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = endpoint
        });

        var postConnectionRequest = new PostToConnectionRequest
        {
            ConnectionId = connectionId,
            Data = stream
        };
        var count = 0;
        try
        {
            stream.Position = 0;
            apiClient.PostToConnectionAsync(postConnectionRequest);
            count++;
        }
        catch(Exception ex)
        {

        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "Data send to " + count + " connection" + (count == 1 ? "" : "s") + "\n"+"Message : "+message
        };
    }
}

public class WebSocketCallMsgBody
{
    public string action { get; set; }
    public string message { get; set; }
}
