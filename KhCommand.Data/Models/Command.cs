using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace KhCommand.Data.Models;

[DebuggerDisplay("'{Name}'")]
public class Command : IEquatable<Command?>
{
    public int CommandId { get; set; }

    public string Name { get; set; } = default!;

    [NotMapped]
    public string NormalizedName => Name.Replace(' ', '_');

    public CommandType CommandType { get; set; }

    public string Description { get; set; } = default!;

    public int MemoryConsumption { get; set; }

    public int OpinionScore { get; set; }

    public string OpinionDescription { get; set; } = default!;

    public bool Shop { get; set; }

    public int? Cost { get; set; }

    public virtual List<Synthesis> Synthesis { get; set; } = [];

    public override bool Equals(object? obj)
    {
        return Equals(obj as Command);
    }

    public bool Equals(Command? other)
    {
        return other is not null &&
               Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }
}
