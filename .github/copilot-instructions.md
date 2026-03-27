# Copilot Instructions

## Project Overview

ASP.NET Core (.NET 10) web application template combining **htmx** (partial page updates), **Alpine.js** (client-side reactivity), and **Razor Components** (static SSR). The pattern avoids Blazor interactivity/SignalR — components render server-side only.

## Build & Run

```bash
# Run from the solution root or Website/
cd Website
dotnet run                 # http on :33110, https on :33150
dotnet watch               # hot reload
dotnet build               # compile check
```

There are no tests in the template yet. OpenAPI/Scalar UI is available at `/scalar/v1` in Development.

## Architecture

```
Website/
  Program.cs              # Middleware pipeline + endpoint registration
  Endpoints.cs            # IEndpoint interface + scanning helper
  Extensions.cs           # HttpRequest extension methods (htmx detection)
  Features/
    App.razor             # Router root
    _Imports.razor        # Global Razor usings for all components
    Shared/
      Components/
        BodyLayout.razor  # Full HTML shell (<html>, <head>, <body>) with all JS/CSS
        PageLayout.razor  # Smart wrapper: returns ChildContent only for htmx requests, full shell otherwise
        Home.razor        # / route (example page)
        ...
  wwwroot/
    css/                  # Bootstrap + common.css
    js/                   # htmx, Alpine.js, htmx-sse extension, components.js
    img/                  # favicon, feather-sprite.svg (icon sprite)
```

### Request Flow

- **Full-page load**: `PageLayout` detects non-htmx request → renders `BodyLayout` (full HTML shell) wrapping the page content.
- **htmx partial request**: `PageLayout` detects `HX-Request: true` → returns only `ChildContent` (bare HTML fragment), no shell.
- This means every page component only needs `<PageLayout>` — it automatically handles both cases.

### API Endpoints

Use the `IEndpoint` pattern for Minimal API endpoints:

```csharp
// Features/SomeFeature/SomeEndpoint.cs
public class SomeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/some-route", Handler);
    }

    static IResult Handler() => Results.Ok();
}
```

Register via `services.AddEndpoints(Assembly.GetExecutingAssembly())` and map with `app.MapEndpoints(routeGroup)` in `Program.cs`. Authentication, authorization, and rate-limiting are pre-scaffolded in `Program.cs` (commented out — uncomment to activate).

## Key Conventions

### Page Components

Every page wraps content in `<PageLayout>` and uses `@page`:

```razor
@page "/my-page"

<PageLayout>
    <PageTitle>My Page</PageTitle>
    <h1>Content here</h1>
</PageLayout>
```

### htmx Detection

Use the `HttpRequest` extension methods from `Extensions.cs`:

```csharp
HttpContext.Request.IsHtmx()                          // HX-Request: true
HttpContext.Request.HxSourceMatches("my-source")      // HX-Source header match
```

These are available in Razor components via the `HttpContext` cascading parameter:

```razor
@code {
    [CascadingParameter] private HttpContext? HttpContext { get; set; }
}
```

### Utility Modal

A global `<dialog id="utilityModal">` is included in every page via `PageLayout`. Target `#utilityModal_content` with htmx to load content into it, and dispatch a `close-modal` custom event to close it:

```html
<button hx-get="/some-partial" hx-target="#utilityModal_content">Open Modal</button>
```

The modal wires up an htmx indicator automatically (`#utilityModal_spinner`).

### Shared Components

- **`<Icon Name="feather-icon-name" Size="24" />`** — renders a Feather icon from the SVG sprite
- **`<Working Id="my-spinner" />`** — htmx loading indicator (Bootstrap badge + spinner); show via `hx-indicator="#my-spinner"`

### Scoped CSS

Component-specific styles go in a `.razor.css` file alongside the component (e.g., `Home.razor.css`). Global styles go in `wwwroot/css/common.css`.

### Asset Fingerprinting

Reference static assets with `@Assets["path/to/file"]` (not `/path/to/file`) to get cache-busted URLs:

```razor
<img src="@Assets["img/favicon.png"]" />
```

### Feature Organization

New features should live in `Features/<FeatureName>/` as vertical slices containing their Razor components, endpoint classes, and scoped CSS together.

### Custom Web Components

`wwwroot/js/components.js` is the place to define custom HTML elements (e.g., `<rating-stars rating="4">`). Register with `customElements.define(...)`.

### Alpine.js

Alpine.js 3.15.8 is loaded on every page and available for lightweight client-side reactivity (`x-data`, `x-show`, `@click`, etc.) without needing a full JS framework.

### Critical .csproj Setting

`<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>` is required for htmx navigation to work. Without it, Blazor throws on htmx-driven navigations.

### Namespace

The root namespace is `Website` (set in `.csproj`). Feature namespaces follow `Website.Features.<FeatureName>`.
