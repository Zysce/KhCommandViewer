using KhCommand.Data;
using KhCommand.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace KhCommandViewer.Components.Pages;

public partial class CommandSearch
{
    [Inject]
    private CommandDbContext DbContext { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<Command> _commands = [];

    private int _value = new();

    async Task OnLoadData(LoadDataArgs args)
    {
        _commands = await DbContext
            .Commands
            .Where(x => EF.Functions.Like(x.Name, $"%{args.Filter}%"))
            .OrderBy(x => x.Name)
            .ToArrayAsync();
    }

    void OnChange(object args)
    {
        if (args is null || args.ToString() == "0")
        {
            return;
        }
        Navigation.NavigateTo($"/command/{args}");
    }
}
