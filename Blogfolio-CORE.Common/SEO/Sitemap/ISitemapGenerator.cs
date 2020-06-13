using System.Collections.Generic;
using System.Xml.Linq;

/* Credits: https://github.com/benfoster/Fabrik.Common */

namespace Blogfolio_CORE.Common.SEO.Sitemap
{
    public interface ISitemapGenerator
    {
        /// <summary>
        /// Generates an xml sitemap file
        /// </summary>
        XDocument GenerateSiteMap(IEnumerable<SitemapItem> items);
    }
}