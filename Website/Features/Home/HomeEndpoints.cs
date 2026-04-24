using Microsoft.AspNetCore.Http.HttpResults;

namespace Website.Features.Home;

public static class HomeEndpoints
{
    public static RouteGroupBuilder MapHomeEndpoints(this RouteGroupBuilder app)
    {
        // Toggle the theme session value and return the new stylesheet link for HTMX to swap in-place.
        app.MapGet("/datetime", () =>
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        });

        return app;
    }
}