using System;
using System.Linq;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Diagnostics;
using Sitecore;
using Sitecore.Links;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Layouts;
using System.Web.Caching;

namespace SitecoreFromArg.SimpleSitemapXml
{
    public class SitemapXmlGenerator : HttpRequestProcessor
    {
        public string sitemapUrl { get; set; }
        public string excludedPaths { get; set; }
        public string cacheTime { get; set; }

        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            if (Context.Site == null || string.IsNullOrEmpty(Context.Site.RootPath.Trim())) return;
            if (Context.Page.FilePath.Length > 0) return;

            if (!args.Url.FilePath.Contains(sitemapUrl)) return;

            // Important to return qualified XML (text/xml) for sitemaps
            args.Context.Response.ClearHeaders();
            args.Context.Response.ClearContent();
            args.Context.Response.ContentType = "text/xml";

            // Checking the cache first
            var sitemapXmlCache = args.Context.Cache["sitemapxml"];
            if (sitemapXmlCache != null)
            {
                args.Context.Response.Write(sitemapXmlCache.ToString());
                args.Context.Response.End();
                return;
            }

            // 
            var options = LinkManager.GetDefaultUrlOptions();
            options.AlwaysIncludeServerUrl = true;

            // Creating the XML Header
            var xml = new XmlTextWriter(args.Context.Response.Output);
            xml.WriteStartDocument();
            xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            // Creating the XML Body
            try
            {
                var items = Context.Database.SelectItems("fast:" + Context.Site.RootPath + "//*");

                foreach (var item in items)
                {
                    if (IsPage(item))
                    {
                        if (!item.Paths.IsContentItem) continue;
                        if (excludedPaths.Split('|').Any(p => item.Paths.ContentPath.Contains(p))) continue;
                        xml.WriteStartElement("url");
                        xml.WriteElementString("loc", LinkManager.GetItemUrl(item, options));
                        xml.WriteElementString("lastmod", item.Statistics.Updated.ToString("yyyy-MM-ddThh:mm:sszzz"));
                        xml.WriteEndElement();
                    }
                }
            }
            finally
            {
                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Flush();

                // Cache XML content
                args.Context.Cache.Add("sitemapxml", xml.ToString(), null,
                              DateTime.Now.AddSeconds(int.Parse(cacheTime)),
                              Cache.NoSlidingExpiration,
                              CacheItemPriority.Normal,
                              null);

                args.Context.Response.Flush();
                args.Context.Response.End();
            }
        }

        /// <summary>
        /// Identify the items with a presentation detail
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns></returns>
        private bool IsPage(Item item)
        {
            var result = false;
            var layoutField = new LayoutField(item.Fields[FieldIDs.LayoutField]);
            if (!layoutField.InnerField.HasValue || string.IsNullOrEmpty(layoutField.Value)) return false;
            var layout = LayoutDefinition.Parse(layoutField.Value);
            foreach (var deviceObj in layout.Devices)
            {
                var device = deviceObj as DeviceDefinition;
                if (device == null) return false;
                if (device.Renderings.Count > 0)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}