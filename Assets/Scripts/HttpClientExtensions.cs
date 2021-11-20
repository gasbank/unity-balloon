using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri,
        HttpContent iContent)
    {
        var method = new HttpMethod("PATCH");
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = iContent
        };

        var response = new HttpResponseMessage();
        try
        {
            response = await client.SendAsync(request);
        }
        catch (TaskCanceledException e)
        {
            Debug.LogError("ERROR: " + e);
        }

        return response;
    }
}