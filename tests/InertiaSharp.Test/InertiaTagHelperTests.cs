using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using InertiaSharp.TagHelpers;

namespace InertiaSharp.Test;

public class InertiaTagHelperTests
{
    /// <summary>
    /// Creates a ViewContext without calling its constructor (bypasses IView dependency).
    /// We only need ViewContext.ViewData for InertiaTagHelper.
    /// </summary>
    private static ViewContext CreateViewContext(string? inertiaPageJson = null)
    {
        // Create uninitialized instance to bypass constructor's IView requirement
        var viewContext = (ViewContext)RuntimeHelpers.GetUninitializedObject(typeof(ViewContext));

        var viewData = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary());

        if (inertiaPageJson is not null)
            viewData["InertiaPage"] = inertiaPageJson;

        typeof(ViewContext)
            .GetProperty(nameof(ViewContext.ViewData))!
            .SetValue(viewContext, viewData);

        return viewContext;
    }

    private static (TagHelperContext context, TagHelperOutput output) CreateTagHelperArgs()
    {
        var context = new TagHelperContext(
            "inertia",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "inertia",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        return (context, output);
    }

    private static string GetPreElementHtml(TagHelperOutput output)
    {
        using var sw = new StringWriter();
        output.PreElement.WriteTo(sw, HtmlEncoder.Default);
        return sw.ToString();
    }

    [Fact]
    public void Process_OutputTagName_IsDiv()
    {
        var tagHelper = new InertiaTagHelper
        {
            ViewContext = CreateViewContext("{\"component\":\"Home\"}")
        };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        Assert.Equal("div", output.TagName);
    }

    [Fact]
    public void Process_SetsIdAttribute_ToApp()
    {
        var tagHelper = new InertiaTagHelper
        {
            ViewContext = CreateViewContext("{\"component\":\"Home\"}")
        };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        Assert.Equal("app", output.Attributes["id"].Value.ToString());
    }

    // Inertia.js v3: page data moves from data-page attribute on the div
    // to a <script type="application/json"> tag in PreElement.
    [Fact]
    public void Process_PreElement_ContainsScriptTagWithPageJson()
    {
        var json = "{\"component\":\"Dashboard\",\"props\":{},\"url\":\"/dashboard\"}";
        var tagHelper = new InertiaTagHelper { ViewContext = CreateViewContext(json) };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        var pre = GetPreElementHtml(output);
        Assert.Contains("type=\"application/json\"", pre);
        Assert.Contains("data-page=\"app\"", pre);
        Assert.Contains(json, pre);
    }

    [Fact]
    public void Process_WhenViewDataHasNoInertiaPage_ScriptContainsEmptyObject()
    {
        var tagHelper = new InertiaTagHelper { ViewContext = CreateViewContext(null) };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        var pre = GetPreElementHtml(output);
        Assert.Contains("{}", pre);
    }

    [Fact]
    public void Process_DivHasNoDataPageAttribute()
    {
        var tagHelper = new InertiaTagHelper { ViewContext = CreateViewContext("{}") };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        Assert.Null(output.Attributes["data-page"]);
    }

    [Fact]
    public void Process_TagMode_IsStartTagAndEndTag()
    {
        var tagHelper = new InertiaTagHelper { ViewContext = CreateViewContext("{}") };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
    }

    [Fact]
    public void Process_DivHasIdAttribute()
    {
        var tagHelper = new InertiaTagHelper { ViewContext = CreateViewContext("{}") };
        var (ctx, output) = CreateTagHelperArgs();

        tagHelper.Process(ctx, output);

        Assert.NotNull(output.Attributes["id"]);
    }
}
