using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosDBCacheLib
{
    public class CosmosDbCache : IDistributedCache
    {
        private readonly CosmosDbCacheOptions _options;
        private readonly DocumentClient _client;

        public CosmosDbCache(IOptions<CosmosDbCacheOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;

            _client = new DocumentClient(
                new Uri(_options.ServiceUri), _options.AuthKey);

            _client.ConnectionPolicy.ConnectionProtocol = Protocol.Tcp;
            _client.ConnectionPolicy.ConnectionMode = ConnectionMode.Direct;

            foreach (var location in _options.PreferredLocations)
            {
                _client.ConnectionPolicy.PreferredLocations.Add(location);
            }
        }

        private Uri GetDocumentUri()
        {
            return UriFactory.CreateDocumentCollectionUri(
                _options.DatabaseName,
                _options.CollectionName);
        }

        public byte[] Get(string key)
        {
            return Task.Run(() => this.GetAsync(key)).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            var uri = this.GetDocumentUri();

            try
            {
                string sql = $"SELECT * FROM c WHERE c.id = '{key}'";
                FeedOptions queryOptions =
                new FeedOptions
                {
                    MaxItemCount = -1,
                    EnableCrossPartitionQuery = false,
                    PopulateQueryMetrics = true,
                    PartitionKey = new PartitionKey(key)
                };
                CosmosDbCacheItem result = _client.CreateDocumentQuery<CosmosDbCacheItem>(uri, queryOptions).Where(i => i.Id == key).AsEnumerable().First();

                return result.ToByteContent();
            }
            catch (Exception d)
            {
                
                return null;
            }
        }

        public void Refresh(string key)
        {
            Task.Run(() => this.RefreshAsync(key)).GetAwaiter().GetResult();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            var uri = this.GetDocumentUri();

            var result = await _client.ReadDocumentAsync<CosmosDbCacheItem>(uri);

            await _client.UpsertDocumentAsync(uri, result.Document);
        }

        public void Remove(string key)
        {
            Task.Run(() => this.RemoveAsync(key)).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            var uri = this.GetDocumentUri();

            await _client.DeleteDocumentAsync(uri);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Task.Run(() => this.SetAsync(key, value, options)).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_options.DatabaseName, _options.CollectionName);

            int ttl = options.AbsoluteExpirationRelativeToNow.HasValue ? (int)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds : 10;
            var document = CosmosDbCacheItem.Build(key, ttl, value);

            await _client.UpsertDocumentAsync(uri, document);
        }
    }
}
