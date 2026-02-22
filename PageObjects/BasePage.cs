using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NLog;
using PlaywrightReqnRollCSharp.Support;
using static Microsoft.Playwright.Assertions;

namespace PlaywrightReqnRollCSharp.PageObjects;

public class BasePage(PlaywrightContext context)
{
    protected IPage _page
    {
        get { return context.CurrentPage; }
    }

    protected Logger Logger { get; } = LogManager.GetCurrentClassLogger();
    protected string? DialogMessage { get; set; }

    private async void page_Dialog_EventHandler_Accept(object sender, IDialog dialog)
    {
        if (string.IsNullOrEmpty(dialog.Type))
        {
            Logger.Warn($"{GetType().Name} - Dialog type is empty, not accepting dialog.");
            return;
        }
        Logger.Info($"{GetType().Name} - Accepting dialog message: {dialog.Message}");
        try
        {
            DialogMessage = dialog.Message;
            await dialog.AcceptAsync();
            await _page.WaitForTimeoutAsync(1000);
        }
        catch (PlaywrightException e)
        {
            Logger.Warn($"{e.Message} - When accepting dialog message: {dialog.Message}");
        }
        _page.Dialog -= page_Dialog_EventHandler_Accept;
    }

    private async void page_Dialog_EventHandler_Dismiss(object sender, IDialog dialog)
    {
        if (string.IsNullOrEmpty(dialog.Type))
        {
            Logger.Warn($"{GetType().Name} - Dialog type is empty, not accepting dialog.");
            return;
        }
        Logger.Info($"{GetType().Name} - Dismissing dialog message: {dialog.Message}");
        DialogMessage = dialog.Message;
        await dialog.DismissAsync();
        _page.Dialog -= page_Dialog_EventHandler_Dismiss;
    }

    protected void SetupDialogHandler(bool accept = true)
    {
        if (accept)
            _page.Dialog += page_Dialog_EventHandler_Accept;
        else
            _page.Dialog += page_Dialog_EventHandler_Dismiss;
    }

    protected void RemoveDialogHandler()
    {
        _page.Dialog -= page_Dialog_EventHandler_Accept;
        _page.Dialog -= page_Dialog_EventHandler_Dismiss;
    }

    public async Task TitleCheck(string title)
    {
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = TestConstants.ExtendedTimeout_45 });
        await Expect(_page).ToHaveTitleAsync(title);
    }

    public async Task ClickOnLink(string link)
    {
        var locator = _page.GetByRole(AriaRole.Link, new() { Name = link });
        await locator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = TestConstants.DefaultTimeout });
        await locator.ClickAsync();
    }

    public async Task<bool> IsElementVisibleAsync(ILocator locator, int timeoutInSeconds)
    {
        await _page.WaitForTimeoutAsync(1000);
        try
        {
            var options = new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutInSeconds * 1000
            };
            await locator.First.WaitForAsync(options);
            return await locator.First.IsEnabledAsync();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to verify element visibility, Locator: {locator}, Error: {ex.Message}");
            return false;
        }
    }

    public ILocator GetElementByFollowingSpanText(ILocator locator, string matchingString)
    {
        var clickableElement = locator.Locator($"//self::*[following-sibling::span[text()='{matchingString}']][1]").Last;
        return clickableElement;
    }

    public ILocator GetElementByFollowingLabelText(ILocator locator, string matchingString)
    {
        var clickableElement = locator.Locator($"//self::*[following-sibling::label[text()='{matchingString}']][1]").Last;
        return clickableElement;
    }

    public ILocator GetElementByFollowingLabelContainsText(ILocator locator, string matchingString)
    {
        var clickableElement = locator.Locator($"//self::*[following-sibling::label[contains(text(),'{matchingString}')]][1]").Last;
        return clickableElement;
    }

    public ILocator GetElementByFollowingText(ILocator locator, string matchingString)
    {
        var clickableElement = locator.Locator($"//self::*[following-sibling::text()='{matchingString}'][1]").Last;
        return clickableElement;
    }

    public ILocator GetElementByFollowingContainsText(ILocator locator, string matchingString)
    {
        var clickableElement = locator.Locator($"//self::*[following-sibling::text()[contains(.,'{matchingString}')]][1]").Last;
        return clickableElement;
    }

    public async Task<ILocator?> GetElementByInputTagValueAsync(ILocator inputlocator, string value)
    {
        var list = await inputlocator.AllAsync();
        foreach (ILocator i in list)
        {
            var elevalue = await i.GetAttributeAsync("value");
            if (elevalue == value)
            {
                return i;
            }
        }
        return null;
    }

    public async Task<ILocator> GetParentElementByChildText(ILocator locator, string matchingString)
    {
        var allElementsWithMatchingString = await locator.Filter(new() { HasText = matchingString }).AllAsync();
        var matchingParent = allElementsWithMatchingString[0];
        foreach (var matchingElement in allElementsWithMatchingString)
        {
            var organText = await matchingElement.InnerTextAsync();
            if (organText.Trim() == matchingString)
            {
                matchingParent = matchingElement;
                break;
            }
        }
        return matchingParent;
    }

    protected ILocator? GetLocatorPropertyFromString(string propertyName)
    {
        PropertyInfo? propertyInfo = GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo is null)
            return null;
        if (!propertyInfo.CanRead || propertyInfo.PropertyType != typeof(ILocator))
            return null;

        return propertyInfo.GetValue(this) as ILocator;
    }

    public async Task CaptureApiResponses(string url, int responseStatus, string responseMethod, ILocator element)
    {
        var results = new ConcurrentDictionary<string, JsonDocument>();
        var lastResponseTime = DateTime.UtcNow;
        var settlingTime = TimeSpan.FromMilliseconds(3000);
        var timeout = TimeSpan.FromSeconds(10);

        _page.Response += async (_, response) =>
        {
            if (response.Url.Contains(url)
                && response.Status == responseStatus
                && response.Request.Method == responseMethod)
            {
                var doc = await response.JsonAsync<JsonDocument>();
                if (doc != null) results.TryAdd(response.Url, doc);
            }
        };

        await element.ClickAsync();
        var start = DateTime.UtcNow;
        do
        {
            await Task.Delay(500);
            if (DateTime.UtcNow - start > timeout)
                throw new TimeoutException("Timed out waiting for API responses");
        } while (DateTime.UtcNow - lastResponseTime < settlingTime);
    }

    public static Dictionary<string, string> FlattenJsonIterative(JsonElement root)
    {
        Dictionary<string, string> dict = [];
        var stack = new Stack<(JsonElement Element, string Path)>();

        stack.Push((root, ""));

        while (stack.Count > 0)
        {
            var (element, path) = stack.Pop();

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        string subPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                        stack.Push((property.Value, subPath));
                    }
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        stack.Push((item, $"{path}[{index}]"));
                        index++;
                    }
                    break;

                default:
                    dict[path] = element.ToString();
                    break;
            }
        }

        return dict;
    }
}
