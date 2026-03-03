using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace InertiaSharp.TagHelpers;

/// <summary>
/// Renders the root Inertia div:
/// <code>
///   &lt;inertia /&gt;
///   →
///   &lt;div id="app" data-page="{...json...}"&gt;&lt;/div&gt;
/// </code>
/// Place this tag helper in your App.cshtml shell view.
/// </summary>
[HtmlTargetElement("inertia")]
public class InertiaTagHelper : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var pageJson = ViewContext.ViewData["InertiaPage"] as string ?? "{}";

        output.TagName = "div";
        output.Attributes.SetAttribute("id", "app");
        output.Attributes.SetAttribute("data-page", pageJson);
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}
