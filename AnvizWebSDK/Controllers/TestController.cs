using Microsoft.AspNetCore.Mvc;
using Anviz.SDK;

namespace AnvizWebSDK.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet(Name = "GetTest")]
        public string Get()
        {
            return "Hello world!";
        }
    }
}
