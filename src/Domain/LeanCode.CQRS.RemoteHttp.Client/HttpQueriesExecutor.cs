using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Serialization;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpQueriesExecutor
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions? serializerOptions;

        public HttpQueriesExecutor(HttpClient client)
            : this(client, null)
        { }

        public HttpQueriesExecutor(
            HttpClient client,
            JsonSerializerOptions? serializerOptions)
        {
            this.client = client;
            this.serializerOptions = serializerOptions;
        }

        public virtual async Task<TResult> GetAsync<TResult>(IRemoteQuery<TResult> query)
        {
            using var content = JsonContent.Create(query, query.GetType());
            using var response = await client
                .PostAsync("query/" + query.GetType().FullName, content);

            response.HandleCommonCQRSErrors<QueryNotFoundException, InvalidQueryException>();

            using var responseContent = await response.Content.ReadAsStreamAsync();
            try
            {
                return await JsonSerializer.DeserializeAsync<TResult>(responseContent, serializerOptions);
            }
            catch (Exception ex)
            {
                throw new MalformedResponseException(ex);
            }
        }
    }
}
