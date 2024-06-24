using System.Diagnostics;

namespace KhCommand.Data.Models;

[DebuggerDisplay("{Command1} + {Command2}")]
public class Synthesis
{
    public int Id { get; set; }

    public int Command1Id { get; set; }

    public virtual Command Command1 { get; set; } = default!;

    public int Command2Id { get; set; }

    public virtual Command Command2 { get; set; } = default!;

    public int ResultId { get; set; }

    public virtual Command CommandResult { get; set; } = default!;
}
