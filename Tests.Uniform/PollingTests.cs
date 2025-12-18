using Apps.Uniform.Events;
using Apps.Uniform.Events.Models;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Uniform.Base;

namespace Tests.Uniform
{
    [TestClass]
    public class PollingTests : TestBase
    {
        [TestMethod]
        public async Task OnCompositionsPublishedAsync_IsSuccess()
        {
            var polling = new CompositionEvents(InvocationContext);

            var webhookRequest = new PollingEventRequest<DateMemory>
            {
                Memory = new DateMemory
                {
                    LastPollingTime = DateTime.UtcNow.AddDays(-1)
                },
            };

            var response = await polling.OnCompositionsPublishedAsync(webhookRequest);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            Assert.IsNotNull(response);
        }
    }
}
