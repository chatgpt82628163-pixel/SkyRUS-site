using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SkyRus.Site.Tests;

public class AvatarTests
{
    [Fact]
    public async Task ProfilePicturesComeFromSkyNetwork()
    {
        using var site = new SiteFactory();
        using var app = site.WithWebHostBuilder(b => b.UseSetting("Site:NetworkUrl", "https://sky.example/"));
        site.Network.Add(1234, "Ivan Petrov", "S1");
        var c = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        Assert.Equal(HttpStatusCode.Redirect, (await c.SignInAsync(site, 1234)).StatusCode);
        var html = await (await c.GetAsync("/cabinet")).Content.ReadAsStringAsync();
        // In the header and on the profile, over the initials (which stay when the network has no picture).
        Assert.Equal(2, html.Split("src=\"https://sky.example/api/v1/members/1234/avatar\"").Length - 1);
        Assert.Contains(">IP<img", html);
    }

    [Fact]
    public async Task WithoutTheNetworkAddressOnlyInitials()
    {
        using var site = new SiteFactory();
        var c = await site.As(1234, "Ivan Petrov", "S1");
        var html = await (await c.GetAsync("/cabinet")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("/avatar\"", html);
        Assert.Contains("<span class=\"avatar\">IP</span>", html);
    }
}
