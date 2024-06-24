using KhCommand.Data.Models;
using Microsoft.AspNetCore.Components;

namespace KhCommandViewer.Components.Pages;

public partial class CommandName
{
    [Parameter]
    public Command Command { get; set; } = default!;
}
