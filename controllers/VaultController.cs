using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace vault.Controllers;


// if not exist http header 'refer' or 'refer' not equal 'https://random-ru.cloudflareaccess.com/'

[ApiController]
public class VaultController : ControllerBase
{
    private readonly ILogger<VaultController> _logger;
    private readonly IFireStoreAdapter _fireStore;


    public const string REQUIRED_AUTH = "_AUTH_";

    public VaultController(ILogger<VaultController> logger, IFireStoreAdapter fireStore)
    {
        _logger = logger;
        _fireStore = fireStore;
    }

    [HttpGet("/@/{space}/{group}/{key}")]
    public async Task<IActionResult> GetAsync(string space, string group, string key)
    {
        var (action, snapshot) = await ValidateAsync(space, group, key);

        if (action is not null) 
            return action;
        if (snapshot is null)
            return StatusCode(418);

        if (snapshot!.ContainsField(REQUIRED_AUTH))
        {
            var (space_token, group_token) = 
                await GetSecurityRules(space, group);

            if (!HttpContext.Request.HasAuth($"{space_token}.{group_token}"))
                return StatusCode(418, new { message = "i am rock." });
        }

        var flatten = snapshot!.ToDictionary();

        return Ok(flatten[key]);
    }
    [HttpPut("/@/{space}/{group}/{key}")]
    public async Task<IActionResult> PutAsync(string space, string group, string key, [FromBody] object body)
    {
        var (action, snapshot) = await ValidateAsync(space, group, key);

        if (action is not null) 
            return action;
        if (snapshot is null)
            return StatusCode(418);

        if (snapshot!.ContainsField(REQUIRED_AUTH))
        {
            var (space_token, group_token) = 
                await GetSecurityRules(space, group);

            if (!HttpContext.Request.HasAuth($"{space_token}.{group_token}"))
                return StatusCode(418, new { message = "i am rock." });
        }

        var recasted = dynamic_recast(body);
        var result = await GetReference(space, group)
            .UpdateAsync(key, recasted);

        return Ok(result.UpdateTime);
    }

    // fuck this shit
    // fix grpc transport casting error with non-poco array troubles
    private object dynamic_recast(object o) 
    {
        if (o is JArray a)
            return a.ToObject<List<object>>();
        if (o is JObject obj)
            return obj.ToObject<Dictionary<string, object>>();
        return null;
    }


    private async Task<(IActionResult?, DocumentSnapshot?)> ValidateAsync(string space, string group, string key)
    {
        if (new [] { space, group, key }.Any(string.IsNullOrEmpty))
            return (BadRequest(new { message = "why u are bulling me?" }), null);
        var snapshot = await GetReference(space, group).GetSnapshotAsync();

        if (!snapshot.Exists)
            return (BadRequest(new { message = "am i of joke to you?" }), null);

        if (!snapshot.ContainsField(key))
            return (BadRequest(new { message = "nigga what?" }), null);

        return (null, snapshot);
    }


    private DocumentReference GetReference(string space, string group) =>
        _fireStore
            .Namespaces
            .Document(space)
            .Collection(group)
            .Document("values");

    // todo, refactoring
    private async Task<(string, string)> GetSecurityRules(string space, string group)
    {
        var snapshot = await _fireStore
            .Namespaces
            .Document("_SECURITY_")
            .GetSnapshotAsync();

        if (!snapshot.Exists)
            return default;

        string t(string x) => snapshot.GetValue<string>($"{x}_token");

        return (t(space), t(group));
    }

    [HttpGet("/")]
    public IActionResult GoToConsole()
        => Redirect("https://console.vault.random.lgbt/");
}



public static class Extensions
{
    public static bool HasAuth(this HttpRequest req,  string value)
    {
        var val = req.Headers.Authorization.ToString();
        return !string.IsNullOrEmpty(val) && val.Equals(value);
    }
}