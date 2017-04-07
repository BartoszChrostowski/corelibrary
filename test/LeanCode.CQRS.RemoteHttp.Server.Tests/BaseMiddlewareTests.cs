using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public abstract class BaseMiddlewareTests
    {
        private readonly Assembly assembly = typeof(BaseMiddlewareTests).GetTypeInfo().Assembly;
        protected readonly StubQueryExecutor query = new StubQueryExecutor();
        protected readonly StubCommandExecutor command = new StubCommandExecutor();
        protected readonly RemoteCQRSMiddleware middleware;

        private readonly string endpoint;
        private readonly string defaultObject;
        private readonly StubServiceProvider serviceProvider;

        public BaseMiddlewareTests(string endpoint, Type defaultObject)
        {
            this.endpoint = endpoint;
            this.defaultObject = defaultObject.FullName;

            middleware = new RemoteCQRSMiddleware(null, assembly);

            var commandHandler = new RemoteCommandHandler(command, assembly);
            var queryHandler = new RemoteQueryHandler(query, assembly);
            this.serviceProvider = new StubServiceProvider(commandHandler, queryHandler);
        }

        protected async Task<(int statusCode, string response)> Invoke(string type = null, string content = "{}", string method = "POST")
        {
            type = type ?? defaultObject;

            var ctx = new StubContext(method, $"/{endpoint}/" + type, content);
            ctx.RequestServices = serviceProvider;
            await middleware.Invoke(ctx);

            var statusCode = ctx.Response.StatusCode;
            var body = (MemoryStream)ctx.Response.Body;
            return (statusCode, Encoding.UTF8.GetString(body.ToArray()));
        }
    }
}