namespace Website;

public static class HttpRequestExtensions
{
    public static bool HxSourceMatches(this HttpRequest? request, string source)
    {
        return request?.Headers["HX-Source"].ToString() == source;
    }   

    public static bool HxSourceMatches(this HttpRequest? request, params string[] source)
    {
        return source.Contains(request?.Headers["HX-Source"].ToString());
    } 

    public static bool IsHtmx(this HttpRequest? request)
    {
        return request?.Headers["HX-Request"].ToString() == "true";
    }        
}