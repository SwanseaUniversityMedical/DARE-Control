
using System;
using System.Collections.Generic;
namespace TRE_API.Models
{

    public class User

{
    public string Name { get; set; }

    public DateTime Expiry { get; set; }

}

public class Tre
{
    public string Name { get; set; }

    public bool Active { get; set; }

    public List<User> Users { get; set; }

}

public class Result

{
    public string Id { get; set; }

    public string Description { get; set; }

    public List<Tre> Tre { get; set; }

}

public class PolicyResult
{
    public Result[] result { get; set; }
}
}

