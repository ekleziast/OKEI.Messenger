using System.Net.Security;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChatSample
{
    [Route("api/[controller]")]
    public class RemoteLogin : Controller
    {
        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
            string login = Request.Headers["login"];
            string password = Request.Headers["password"];
        }
    }
}
