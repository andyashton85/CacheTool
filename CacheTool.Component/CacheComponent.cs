using NLog;

namespace CacheTool.Component;

public class CacheComponent : ICacheComponent
{
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private Dictionary<string, object> _cacheDict;
    private int _maxKeys;
	
    private LinkedList<string> _recentlyUsedKeys;

    // lock for thread safety
	private readonly object _lock = new();

	public CacheComponent(int cacheMax = 100)
    {
		_maxKeys = cacheMax;

		_cacheDict = new Dictionary<string, object>(cacheMax);
		_recentlyUsedKeys = new LinkedList<string>();
    }


	public Task<IEnumerable<string>> GetAllKeysAsync()
	{
		try
		{
			if (!_recentlyUsedKeys.Any())
				return Task.FromResult(Enumerable.Empty<string>());

			return Task.FromResult(_recentlyUsedKeys.AsEnumerable());
		}
		catch (Exception ex)
		{
			_logger.Error(ex, ex.Message);
			
			return Task.FromResult(Enumerable.Empty<string>());
		}
	}

	public Task<bool> TryGetAsync<T>(string cacheKey, out T value)
    {
        try
        {
            value = default;

			if (!_cacheDict.TryGetValue(cacheKey, out var val))
				return Task.FromResult(false);

			// Make thread safe
			lock (_lock)
			{
				_recentlyUsedKeys.Remove(cacheKey);
				_recentlyUsedKeys.AddFirst(cacheKey);
			}

			value = (T)val;

			return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);

			value = default;
			return Task.FromResult(false);
		}
    }

    public Task<Enums.CacheResult> TryStoreAsync<T>(string cacheKey, T value)
    {
        try
        {
			var result = Enums.CacheResult.Added;

			String oldestKey = null;
			// Make thread safe
			lock (_lock)
			{
				if (_cacheDict.ContainsKey(cacheKey))
				{
					// Update for existing key
					_cacheDict[cacheKey] = value;
					result = Enums.CacheResult.Updated;
					_logger.Info($"Key \"{cacheKey}\" updated");
				}
				else
				{
					// Insert
					if (_cacheDict.Count == _maxKeys)
					{
						oldestKey = _recentlyUsedKeys.LastOrDefault();

						_cacheDict.Remove(oldestKey);

						// Return Cache Overflow indicator
						result = Enums.CacheResult.CacheOverflow;
					}

					_cacheDict.Add(cacheKey, value);
					_logger.Info($"Key \"{cacheKey}\" added");
				}

				_recentlyUsedKeys.Remove(oldestKey ?? cacheKey);
				_recentlyUsedKeys.AddFirst(cacheKey);
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
			_logger.Error(ex, ex.Message);
			
			return Task.FromResult(Enums.CacheResult.KeyNotFoundException);
        }
    }

	public Task<bool> TryRemoveAsync(string cacheKey)
	{
        try
        {
			if (_cacheDict.Any(x => x.Key == cacheKey))
			{
				// Make thread safe
				lock (_lock)
				{
					_cacheDict.Remove(cacheKey);
					_recentlyUsedKeys.Remove(cacheKey);
				}

				_logger.Info($"Key \"{cacheKey}\" removed");
				return Task.FromResult(true);
			}

			return Task.FromResult(false);
		}
        catch (Exception ex)
        {
			_logger.Error(ex, ex.Message);
			
			return Task.FromResult(false);
		}
	}

	public Task EmptyAsync()
	{
        try
        {
			_cacheDict.Clear();

			_recentlyUsedKeys.Clear();

			_logger.Info("Cache emptied");
			return Task.CompletedTask;
        }
        catch (Exception ex)
        {
			_logger.Error(ex, ex.Message);
            return Task.FromException(ex);
		}
	}
}
