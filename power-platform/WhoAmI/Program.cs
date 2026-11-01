using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var envUrl = config["Dataverse:EnvUrl"]!.TrimEnd('/');

var credential = new ClientSecretCredential(
    config["Dataverse:TenantId"], config["Dataverse:ClientId"], config["Dataverse:ClientSecret"]);

var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{envUrl}/.default" }));

using var http = new HttpClient { BaseAddress = new Uri($"{envUrl}/api/data/v9.2/") };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
http.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
http.DefaultRequestHeaders.Add("OData-Version", "4.0");
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var response = await http.GetAsync("WhoAmI");
Console.WriteLine((int)response.StatusCode);
Console.WriteLine(await response.Content.ReadAsStringAsync());



// Test for other endpoints
var r = await http.GetAsync("contacts?$top=1&$select=contactid");
Console.WriteLine($"contacts: {(int)r.StatusCode}");


// Test for upsert
var body = """
{
  "hl_name": "pi_test_1",
  "hl_amount": 25.00,
  "hl_currency": "NZD",
  "hl_donatedon": "2026-10-31T08:00:00Z"
}
""";

async Task<HttpResponseMessage> Upsert()
{
    var req = new HttpRequestMessage(HttpMethod.Patch, "hl_donations(hl_providerpaymentid='pi_test_2')")
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };
   req.Headers.TryAddWithoutValidation("If-None-Match", "null");

    Console.WriteLine(string.Join("\n", req.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
    // Console.WriteLine(string.Join("\n", http.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
    return await http.SendAsync(req);
}

async Task<HttpResponseMessage> Create()
{
    var req = new HttpRequestMessage(HttpMethod.Patch, "hl_donations(hl_providerpaymentid='pi_test_5')")
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };
   req.Headers.TryAddWithoutValidation("If-None-Match", "*");

    Console.WriteLine(string.Join("\n", req.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
    // Console.WriteLine(string.Join("\n", http.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
    return await http.SendAsync(req);
}

var first = await Upsert();
Console.WriteLine($"first:  {(int)first.StatusCode}");
var second = await Upsert();
Console.WriteLine($"second: {(int)second.StatusCode}");
Console.WriteLine(await second.Content.ReadAsStringAsync());


var third = await Create();
Console.WriteLine($"third:  {(int)third.StatusCode}");
Console.WriteLine(await third.Content.ReadAsStringAsync());


async Task<HttpResponseMessage> Post(string paymentId)
{
    var json = $$"""
    {
      "hl_name": "{{paymentId}}",
      "hl_amount": 25.00,
      "hl_currency": "NZD",
      "hl_donatedon": "2026-10-31T08:00:00Z",
      "hl_providerpaymentid": "{{paymentId}}"
    }
    """;
    var req = new HttpRequestMessage(HttpMethod.Post, "hl_donations")
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
    return await http.SendAsync(req);
}

var p1 = await Post("pi_test_6");
Console.WriteLine($"post 1: {(int)p1.StatusCode}");
var p2 = await Post("pi_test_6");
Console.WriteLine($"post 2: {(int)p2.StatusCode}");
Console.WriteLine(await p2.Content.ReadAsStringAsync());