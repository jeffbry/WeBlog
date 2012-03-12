﻿using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Publishing;

namespace Sitecore.Modules.WeBlog
{
    public class ContentHelper
    {
        /// <summary>
        /// Publishes the item.
        /// </summary>
        /// <param name="targetItemID">The target item ID.</param>
        /// <param name="childs">If true publish the children of the item as well</param>
        public static void PublishItem(ID targetItemID, bool childs = false)
        {
            Sitecore.Data.Database master = GetContentDatabase();
            Sitecore.Data.Items.Item home = master.GetItem(targetItemID);
            PublishItem(home, false);
        }

        /// <summary>
        /// Publishes the item.
        /// </summary>
        /// <param name="targetItem">The target item.</param>
        public static void PublishItem(Item targetItem, bool childs = false)
        {
            DateTime publishDate = DateTime.Now;
            Sitecore.Data.Database master = GetContentDatabase();
            Sitecore.Data.Items.Item targets = master.GetItem(Constants.Paths.PublishingTargets);
            foreach (Sitecore.Data.Items.Item target in targets.Children)
            {
                string targetDBName = target["target database"];
                Sitecore.Data.Database targetDB = Sitecore.Configuration.Factory.GetDatabase(targetDBName);
                foreach (Sitecore.Globalization.Language language in master.Languages)
                {
                    Sitecore.Publishing.PublishOptions publishOptions = new Sitecore.Publishing.PublishOptions(master, targetDB, Sitecore.Publishing.PublishMode.SingleItem, language, publishDate);
                    publishOptions.RootItem = targetItem;
                    publishOptions.Deep = childs;
                    Sitecore.Publishing.Publisher publisher = new Sitecore.Publishing.Publisher(publishOptions); publisher.Publish();
                }
            }
        }

        /// <summary>
        /// Publish the item and recursivley any ancestors that haven't yet been published
        /// </summary>
        /// <param name="item">The item to publish</param>
        public static void PublishItemAndRequiredAncestors(Item item, Database targetDatabase)
        {
            if (item != null)
            {
                var ancestorInTarget = targetDatabase.GetItem(item.ParentID);
                if (ancestorInTarget == null)
                {
                    PublishItemAndRequiredAncestors(item.Parent, targetDatabase);
                }

                ancestorInTarget = targetDatabase.GetItem(item.ParentID);
                if (ancestorInTarget != null)
                {
                    foreach (Sitecore.Globalization.Language language in item.Database.Languages)
                    {
                        Sitecore.Publishing.PublishOptions publishOptions = new Sitecore.Publishing.PublishOptions(item.Database, targetDatabase, Sitecore.Publishing.PublishMode.SingleItem, language, DateTime.Now);
                        publishOptions.RootItem = item;
                        publishOptions.Deep = false;
                        Sitecore.Publishing.Publisher publisher = new Sitecore.Publishing.Publisher(publishOptions);
                        publisher.Publish();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the content database
        /// </summary>
        /// <returns>The content database</returns>
        public static Database GetContentDatabase()
        {
            var site = Sitecore.Configuration.Factory.GetSite(Sitecore.Constants.ShellSiteName);
            return site.ContentDatabase;
        }
    }
}