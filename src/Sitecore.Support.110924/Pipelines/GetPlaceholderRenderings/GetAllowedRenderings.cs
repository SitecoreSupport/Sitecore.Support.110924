using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.Text;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;


namespace Sitecore.Support.Pipelines.GetPlaceholderRenderings
{

  public class GetAllowedRenderings
  {
    /// <summary>
	/// Fills placeholder renderings list.
	/// </summary>
	/// <param name="args">The arguments.</param>
	public void Process(GetPlaceholderRenderingsArgs args)
    {
      Assert.IsNotNull(args, "args");
      Item item = null;
      if (ID.IsNullOrEmpty(args.DeviceId))
      {
        SecurityDisabler val = new SecurityDisabler();
        try
        {
          item = Client.Page.GetPlaceholderItem(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
        }
        finally
        {
          val.Dispose();
        }
      }
      else
      {
        using (new DeviceSwitcher(args.DeviceId, args.ContentDatabase))
        {
          SecurityDisabler val = new SecurityDisabler();
          try
          {
            item = Client.Page.GetPlaceholderItem(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
          }
          finally
          {
            val.Dispose();
          }
        }
      }
      List<Item> list = null;
      if (item != null)
      {
        args.HasPlaceholderSettings = true;
        bool allowedControlsSpecified;
        list = GetRenderings(item, out allowedControlsSpecified);
        if (allowedControlsSpecified)
        {
          args.Options.ShowTree = false;
        }
      }
      if (list != null)
      {
        if (args.PlaceholderRenderings == null)
        {
          args.PlaceholderRenderings = new List<Item>();
        }
        args.PlaceholderRenderings.AddRange(list);
      }
    }

    /// <summary>
    /// Gets the allowed renderings.
    /// </summary>
    /// <param name="placeholderKey">The placeholder key.</param>
    /// <param name="layoutDefinition">The layout definition.</param>
    /// <param name="contentDatabase">The content database.</param>
    /// <returns>The allowed renderings.</returns>
    [Obsolete("Deprecated")]
    protected virtual List<Item> GetRenderings(string placeholderKey, string layoutDefinition, Database contentDatabase)
    {
      Assert.IsNotNull(placeholderKey, "placeholder");
      Assert.IsNotNull(contentDatabase, "database");
      Assert.IsNotNull(layoutDefinition, "layout");
      Item placeholderItem = Client.Page.GetPlaceholderItem(placeholderKey, contentDatabase, layoutDefinition);
      if (placeholderItem == null)
      {
        return null;
      }
      bool allowedControlsSpecified;
      return GetRenderings(placeholderItem, out allowedControlsSpecified);
    }

    /// <summary>
    /// Gets the renderings.
    /// </summary>
    /// <param name="placeholderItem">The placeholder item.</param>
    /// <param name="allowedControlsSpecified">A value indicating if allowed condtrols are specified in the placeholder item.</param>
    /// <returns>The renderings.</returns>
    protected virtual List<Item> GetRenderings(Item placeholderItem, out bool allowedControlsSpecified)
    {
      Assert.ArgumentNotNull(placeholderItem, "placeholderItem");
      allowedControlsSpecified = false;
      ListString listString = new ListString(placeholderItem["Allowed Controls"]);
      if (listString.Count <= 0)
      {
        return null;
      }
      allowedControlsSpecified = true;
      List<Item> list = new List<Item>();
      foreach (string item2 in listString)
      {
        Item item = placeholderItem.Database.GetItem(item2);
        if (item != null)
        {
          list.Add(item);
        }
      }
      return list;
    }
  }
}