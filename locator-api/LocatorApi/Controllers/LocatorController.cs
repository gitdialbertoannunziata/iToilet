using System.Net;
using Dapr.Client;

using LocatorApi.Entities;
using LocatorApi.Models;
using LocatorApi.Repository;

using Microsoft.AspNetCore.Mvc;

namespace LocatorApi.Controllers
{
    [Route("")]
    [ApiController]
    public class LocatorController : ControllerBase
    {
        private readonly DaprClient daprClient;
        private readonly IToiletRepository repository;
        private readonly string ReviewAppId = "Review";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LocatorController> logger;

        public LocatorController(DaprClient daprClient, IToiletRepository repository, IHttpContextAccessor httpContextAccessor, ILogger<LocatorController> logger)
        {
            this.daprClient = daprClient;
            this.repository = repository;
            _httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        [HttpGet]
        [Route("around/{city}/{lat}/{lng}")]
        [ProducesResponseType(typeof(List<ToiletWithReview>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Around(string city, double lat, double lng)
        {
            var ret = new List<ToiletWithReview>();
            var nearest = await repository.GetNearestAsync(city, lat, lng, _httpContextAccessor?.HttpContext?.RequestAborted ?? default);
            await Parallel.ForEachAsync(nearest, _httpContextAccessor?.HttpContext?.RequestAborted ?? default, async (toilet, token) =>
            {
                try
                {
                    ToiletWithReview t = new(toilet);
                    var requestMessage = daprClient.CreateInvokeMethodRequest(HttpMethod.Get, ReviewAppId, $"GetAllByToilet/{toilet.Id}");
                    var responseMessage = await daprClient.InvokeMethodWithResponseAsync(requestMessage, _httpContextAccessor?.HttpContext?.RequestAborted ?? default);
                    if (responseMessage != null && responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        t.Reviews = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<Review>>();
                    }
                    ret.Add(t);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Review error");
                }
            });

            return Ok(ret);
        }


        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAsync([FromBody] ToiletModel toiletToAdd)
        {
            var toilet = new Toilet()
            {
                Id = toiletToAdd.Id,
                Address = toiletToAdd.Address,
                City = toiletToAdd.City,
                Name = toiletToAdd.Name,
                Photo = toiletToAdd.Photo,
                Point = new Microsoft.Azure.Cosmos.Spatial.Point(toiletToAdd.Point.Longitude, toiletToAdd.Point.Latitude)
            };

            return NoContent();
        }
    }
}

