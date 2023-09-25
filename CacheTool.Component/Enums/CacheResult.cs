namespace CacheTool.Component.Enums
{
	public enum CacheResult
	{
		Retrieved = 1, // For future usage
		Added = 2,
		Updated = 3,
		Deleted = 4,  // For future usage
		Cleared = 5,  // For future usage
		CacheOverflow = 6,

		KeyNotFoundException = 400
	}
}
