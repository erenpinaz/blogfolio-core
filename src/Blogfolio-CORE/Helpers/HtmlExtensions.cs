using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Linq.Expressions;

namespace Blogfolio_CORE.Helpers
{
    public static class HtmlExtensions
    {
        private static IHtmlContent MetaDataFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Func<ModelMetadata, string> property)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            ModelExpressionProvider modelExpressionProvider = (ModelExpressionProvider)html.ViewContext.HttpContext.RequestServices.GetService(typeof(IModelExpressionProvider));

            var modelExplorer = modelExpressionProvider.CreateModelExpression(html.ViewData, expression);
            if (modelExplorer == null) throw new InvalidOperationException($"Failed to get model explorer for {modelExpressionProvider.GetExpressionText(expression)}");

            return new HtmlString(property(modelExplorer.Metadata));
        }

        public static IHtmlContent DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.MetaDataFor(expression, m => m.Description);
        }
    }
}
