using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhCommand.Data.Utils;

public class ImageExtractor
{
    private const string _url = "https://kingdomhearts.fandom.com/wiki/Command_Matrix";
    
    private readonly HttpClient _httpClient;
    private HtmlDocument _document;

    public ImageExtractor()
    {
        _httpClient = new HttpClient();
        _document = new HtmlDocument();
    }

    public async Task Load()
    {
        var page = await File.ReadAllTextAsync("Seed/command_html.txt");
        _document.LoadHtml(page);
    }

    public async Task Extract(string name)
    {
        var idifiedName = name.Replace(' ', '_');
        var elt = _document
            .GetElementbyId(idifiedName);
        var sib = elt.PreviousSibling.PreviousSibling;
        var href = sib.GetAttributeValue("href", string.Empty);
        var response = await _httpClient.GetAsync(href);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync($"wwwroot/images/{idifiedName}.png", bytes);
    }
}
