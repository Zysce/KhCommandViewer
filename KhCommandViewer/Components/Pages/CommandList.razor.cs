using KhCommand.Data;
using KhCommand.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace KhCommandViewer.Components.Pages;

public partial class CommandList
{
    [Inject]
    private CommandDbContext DbContext { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IGrouping<CommandType, Command>[] _commands = [];

    protected override async Task OnInitializedAsync()
    {
        var commands = await DbContext.Commands.ToListAsync();

        _commands = commands.GroupBy(x => x.CommandType).ToArray();
    }

    private void NavigateToPage(int id)
    {
        Navigation.NavigateTo($"/command/{id}");
    }
}
