using CacheTool.Component;
using CacheTool.Component.Enums;
using CacheTool.Models;
//using Moq;

namespace CacheTool.Tests
{
	[TestFixture]
	public class NUnitTests
	{
		private TestModel _preexistingTestModel;
		private TestModel _newTestModel;

		private const int cacheCapacity = 3;
		private ICacheComponent _testCacheComponent;

		//private ITestService _testService;

		[SetUp]
		public void Setup()
		{
			// To Mock Services
			//_testService = new Mock<ITestService>(MockBehavior.Strict);

			_testCacheComponent = new CacheComponent(cacheCapacity);

			_preexistingTestModel = new TestModel { Id = 1, Name = "Obj 1" };
			_testCacheComponent.TryStoreAsync("CacheKey1", _preexistingTestModel);
			_newTestModel = new TestModel { Id = 2, Name = "New Object" };
		}

		[TestCase("CacheKey1", CacheResult.Updated)]
		[TestCase("CacheKey2", CacheResult.Added)]
		public void Store_Always_ReturnsExpectedResult(string key, CacheResult expectedResult)
		{
			TestModel model = _preexistingTestModel;
			if (key != "CacheKey1")
				model = _newTestModel;

			Assert.That(_testCacheComponent.TryStoreAsync(key, model).Result, Is.EqualTo(expectedResult));

			//_testService.VerifyAll();
		}

		[TestCase("CacheKey1", true)]
		[TestCase("CacheKey0", false)]
		public void Get_Always_ReturnsExpectedResult(string key, bool expectedResult)
		{
			TestModel retrievedTestModel;
			Assert.That(_testCacheComponent.TryGetAsync(key, out retrievedTestModel).Result, Is.EqualTo(expectedResult));

			//_testService.VerifyAll();
		}

		[TestCase(CacheResult.CacheOverflow)]
		public void CacheOverflow_ReturnsExpectedResult(CacheResult expectedResult)
		{
			CacheResult result = CacheResult.KeyNotFoundException;

			var mockCacheComponent = new CacheComponent(cacheCapacity);

			var i = 0;
			while (i <= cacheCapacity)
			{
				result = mockCacheComponent.TryStoreAsync($"CacheKey{i}", new TestModel()).Result;
				i++;
			}
			
			Assert.That(result, Is.EqualTo(expectedResult));

			//_testService.VerifyAll();
		}
	}
}
