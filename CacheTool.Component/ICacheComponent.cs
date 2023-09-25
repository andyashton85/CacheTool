namespace CacheTool.Component
{
    public interface ICacheComponent
    {
		Task<IEnumerable<string>> GetAllKeysAsync();
		Task<bool> TryGetAsync<T>(string cacheKey, out T value);
        Task<Enums.CacheResult> TryStoreAsync<T>(string cacheKey, T value);
		Task<bool> TryRemoveAsync(string cacheKey);
		Task EmptyAsync();
	}
}
