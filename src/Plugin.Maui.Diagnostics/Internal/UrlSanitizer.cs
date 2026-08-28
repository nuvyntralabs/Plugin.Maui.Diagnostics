namespace Plugin.Maui.Diagnostics;

static class UrlSanitizer
{
    public static string Sanitize(string url, bool redactQuery)
    {
        if (string.IsNullOrWhiteSpace(url) || !redactQuery)
        {
            return url;
        }

        var hash = url.IndexOf('#');
        var query = url.IndexOf('?');
        if (query < 0)
        {
            return url;
        }

        if (hash > query)
        {
            return string.Concat(url.AsSpan(0, query), url.AsSpan(hash));
        }

        return url[..query];
    }
}
