namespace ArcaneVaultRP.Helpers
{
    public static class HttpClientHelper
    {
        public static void AttachJwtToken(HttpClient httpClient, string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}