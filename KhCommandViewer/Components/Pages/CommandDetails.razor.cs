using KhCommand.Data;
using KhCommand.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace KhCommandViewer.Components.Pages;

public partial class CommandDetails
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    private CommandDbContext DbContext { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private Command? _command;
    private Command[] _ingredients = [];

    protected override async Task OnParametersSetAsync()
    {
        _command = await DbContext
            .Commands
            .Include(x => x.Synthesis)
                .ThenInclude(x => x.Command1)
            .Include(x => x.Synthesis)
                .ThenInclude(x => x.Command2)
            .FirstOrDefaultAsync(x => x.CommandId == Id);

        _ingredients = await DbContext
                .Synthesises
                .Include(x => x.Command1)
                .Include(x => x.Command2)
                .Include(x => x.CommandResult)
                .Where(x => x.Command1.Name == _command.Name || x.Command2.Name == _command.Name)
                .Select(x => x.CommandResult)
                .Distinct()
                .ToArrayAsync();
    }

    private void NavigateToPage(int id)
    {
        Navigation.NavigateTo($"/command/{id}");
    }
}
