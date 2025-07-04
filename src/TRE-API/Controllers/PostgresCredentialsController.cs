﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace TRE_API.Controllers
{
    /// <summary>
    /// API controller for managing postgres credentials and authentication operations
    /// </summary>
    [Route("api/tre-credentials/postgres")]
    [ApiController]
    [SwaggerTag("Postgrescredentials", "Manage Postgres Database Credentials")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class PostgresCredentialsController : ControllerBase
    {
    }
}
