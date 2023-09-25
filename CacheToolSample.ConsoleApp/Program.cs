/* Andy Ashton
 * 
 * Notes -
 * I would also generally utilise common/useful tools such as AutoMapper, Dapper ORM (or EF Core), etc
 */


using CacheTool.Component;
using CacheTool.Component.Enums;
using CacheTool.Models;
using Autofac;

Console.WriteLine("--- BEGIN ---");
Console.WriteLine();

// Initialise / Set up cache tool
var builder = new ContainerBuilder();

// Register individual components (Set max capacity to 5)
builder.RegisterInstance(new CacheComponent(5))
	   .As<ICacheComponent>();

var container = builder.Build();


var cacheComponent = container.Resolve<ICacheComponent>();


// Add to Cache
var storeTest = await cacheComponent.TryStoreAsync<TestModel>("CacheKey1", new TestModel { Id = 1, Name = "Item 1" });
if (storeTest == CacheResult.Added)
	Console.WriteLine("Added to Cache (CacheKey1)");

Console.WriteLine();

// Retrieve from Cache
var getTest = (TestModel)null;
await cacheComponent.TryGetAsync<TestModel>("CacheKey1", out getTest);
if (getTest != null)
	Console.WriteLine($"CacheKey1 = TestModel Id : {getTest.Id}, Name : {getTest.Name}");

Console.WriteLine();

// Remove from Cache
var removeTest = await cacheComponent.TryRemoveAsync("CacheKey1");
if (removeTest)
	Console.WriteLine("Cache Item Removed (CacheKey1)");

Console.WriteLine();
Console.WriteLine();

// Simulate Cache Overflow
Console.WriteLine("Cache Overflow Test - 5 Items (Cache Keys) Maximum ::");
Console.WriteLine();

var cacheKey = "";
TestModel model;
for (int i = 0; i < 6; i++)
{
	cacheKey = $"CacheKey{i + 1}";
	model = new TestModel { Id = (i + 1), Name = $"Object {i + 1}" };

	var storeResult = await cacheComponent.TryStoreAsync<TestModel>(cacheKey, model);
	if (storeResult == CacheResult.CacheOverflow)
		Console.WriteLine("WARNING: CACHE LIMIT EXCEEDED.");

	await cacheComponent.TryGetAsync<TestModel>(cacheKey, out model);
	Console.WriteLine($"Added: {cacheKey} = TestModel Id : {model.Id}, Name : {model.Name}");

	Console.WriteLine();
}

Console.WriteLine("Cacke Keys =");
Console.WriteLine($"{string.Join(Environment.NewLine, cacheComponent.GetAllKeysAsync().Result)}");

Console.WriteLine();
Console.WriteLine("--- END ---");

Console.ReadKey();
