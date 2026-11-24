using Apps.Uniform.Events;
using Apps.Uniform.Events.Models;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Uniform.Base;

namespace Tests.Uniform
{
    [TestClass]
    public class PollingTests : TestBase
    {
        private static PollingEventRequest<DateMemory> CreateRequestSince(DateTime lastPollingTime) => new()
        {
            Memory = new DateMemory
            {
                LastPollingTime = lastPollingTime
            },
        };

        [TestMethod]
        public async Task OnCompositionsPublishedAsync_IsSuccess()
        {
            var polling = new CompositionEvents(InvocationContext);

            var response = await polling.OnCompositionsPublishedAsync(CreateRequestSince(DateTime.UtcNow.AddDays(-1)));

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task OnCompositionsPublishedAsync_AllPublishedCompositions_ReturnsMetadataWithoutServerError()
        {
            var polling = new CompositionEvents(InvocationContext);

            var response = await polling.OnCompositionsPublishedAsync(CreateRequestSince(DateTime.UtcNow.AddYears(-10)));

            Assert.IsTrue(response.FlyBird);
            Assert.IsTrue(response.Result!.Items.All(composition => !string.IsNullOrEmpty(composition.Id) && !string.IsNullOrEmpty(composition.Name)));
            Console.WriteLine($"Published compositions: {response.Result.TotalCount}");
        }

        [TestMethod]
        public async Task OnCompositionsUpdatedAsync_DraftPageLargerThanUniformResponseLimit_ReturnsMetadataWithoutServerError()
        {
            var polling = new CompositionEvents(InvocationContext);

            var response = await polling.OnCompositionsUpdatedAsync(
                CreateRequestSince(DateTime.UtcNow.AddYears(-10)),
                new CompositionFilters { State = "0" });

            Assert.IsTrue(response.FlyBird);
            Assert.IsTrue(response.Result!.Items.All(composition => !string.IsNullOrEmpty(composition.Id) && !string.IsNullOrEmpty(composition.Name)));
            Console.WriteLine($"Updated draft compositions: {response.Result.TotalCount}");
        }
    }
}
