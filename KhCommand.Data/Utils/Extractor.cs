using KhCommand.Data;
using KhCommand.Data.Models;
using KhCommand.Data.Utils;
using System.Text.RegularExpressions;

namespace KHCommandExtractor;

public partial class Extractor(CommandDbContext dbContext)
{
    private const string _filePath = "Seed/khrec_commands.txt";

    private readonly CommandDbContext _dbContext = dbContext;

    [GeneratedRegex("(?<name>[a-zA-Z0-9_ ]*) \\(Memory Consumption: (?<mem>\\d+)%\\)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex NameMemRegex();

    [GeneratedRegex("[a-zA-Z0-9_ ]+(Shop): (?<cost>\\d+) Munny", RegexOptions.IgnoreCase, "en-Us")]
    private static partial Regex CostShopRegex();

    [GeneratedRegex("Opinion: ((?<score>\\d)\\/5 (?<description>[a-zA-Z0-9_ ,]+)|(See (?<see>[a-zA-Z0-9_ ,]+)))", RegexOptions.IgnoreCase, "en-Us")]
    private static partial Regex OpinionRegex();

    [GeneratedRegex("(?<cmd1>[a-zA-Z0-9_ ]+) \\+ (?<cmd2>[a-zA-Z0-9_ ]+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SynthesisRegex();

    public async Task Extract()
    {
        var lines = await File.ReadAllLinesAsync(_filePath);

        var cmds = BuildList(lines);

        await SaveInDb(cmds);
    }

    private async Task SaveInDb(List<Command> cmds)
    {
        await _dbContext.Database.EnsureCreatedAsync();

        await _dbContext.AddRangeAsync(cmds);
        await _dbContext.SaveChangesAsync();
    }

    private List<Command> BuildList(string[] lines)
    {
        var cmds = new List<Command>();
        var commandType = CommandType.Physical;
        bool cmdType = false;

        string name = string.Empty;
        string description = string.Empty;
        int memoryConsumption = 0;
        int opinionScore = 0;
        string opinionDesc = string.Empty;
        bool shop = false;
        int? cost = null;
        var synth = new Dictionary<string, List<(string, string)>>();

        foreach (var line in lines)
        {
            if (line.StartsWith("---"))
            {
                cmdType = true;

                if (line.StartsWith("---Magical"))
                {
                    commandType = CommandType.Magical;
                }

                continue;
            }

            if (!cmdType && line == string.Empty)
            {
                var cmd = CreateNewCommand(commandType, name, description, memoryConsumption, opinionScore, opinionDesc, shop, cost);
                cmds.Add(cmd);
                cost = null;
                shop = false;
                continue;
            }

            if (NameMemRegex().IsMatch(line))
            {
                cmdType = false;
                name = NameMemRegex().Match(line).Groups["name"].Value.Trim();
                memoryConsumption = int.Parse(NameMemRegex().Match(line).Groups["mem"].Value);
                continue;
            }

            if (line.StartsWith('-'))
            {
                cmdType = false;
                description = line[1..];
                continue;
            }

            if (CostShopRegex().IsMatch(line))
            {
                cmdType = false;
                cost = int.Parse(CostShopRegex().Match(line).Groups["cost"].Value);
                shop = true;
                continue;
            }

            if (line == "SYNTH ONLY")
            {
                cmdType = false;
                cost = null;
                shop = false;
                continue;
            }

            if (line == "N/A")
            {
                cmdType = false;
                continue;
            }

            if (OpinionRegex().IsMatch(line))
            {
                cmdType = false;
                var see = OpinionRegex().Match(line).Groups["see"].Value;
                if (!string.IsNullOrWhiteSpace(see))
                {
                    var otherCmd = cmds.SingleOrDefault(c => c.Name == see);
                    if (otherCmd != null)
                    {
                        opinionDesc = otherCmd.OpinionDescription;
                        opinionScore = otherCmd.OpinionScore;
                    }
                    else
                    {

                        Console.WriteLine($"Unable to find for opinion {see}");
                        opinionDesc = see;
                        opinionScore = -1;
                    }
                }
                else
                {
                    opinionDesc = OpinionRegex().Match(line).Groups["description"].Value;
                    opinionScore = int.Parse(OpinionRegex().Match(line).Groups["score"].Value);
                }

                continue;
            }

            if (SynthesisRegex().IsMatch(line))
            {
                cmdType = false;
                if (!synth.ContainsKey(name))
                {
                    synth.Add(name, []);
                }

                var cmd1 = SynthesisRegex().Match(line).Groups["cmd1"].Value.Trim();
                var cmd2 = SynthesisRegex().Match(line).Groups["cmd2"].Value.Trim();

                synth[name].Add((cmd1, cmd2));
            }
        }

        foreach (var kvp in synth)
        {
            var result = kvp.Key;
            var resultCmd = cmds.SingleOrDefault(x => x.Name == result);
            if (resultCmd == null)
            {
                Console.WriteLine($"Unable to find result {result}");
                continue;
            }
            foreach (var synthRecipe in kvp.Value)
            {
                var cmd1 = cmds.SingleOrDefault(x => x.Name == synthRecipe.Item1);
                var cmd2 = cmds.SingleOrDefault(x => x.Name == synthRecipe.Item2);

                if (cmd1 == null || cmd2 == null)
                {
                    if (cmd1 == null)
                    {
                        Console.WriteLine($"Unable to find cmd1 {synthRecipe.Item1}");
                    }

                    if (cmd2 == null)
                    {
                        Console.WriteLine($"Unable to find cmd2 {synthRecipe.Item2}");
                    }

                    continue;
                }

                resultCmd.Synthesis.Add(new Synthesis { Command1 = cmd1, Command2 = cmd2, CommandResult = resultCmd });
            }
        }

        foreach (var cmd in cmds.Where(cmd => cmd.OpinionScore == -1))
        {
            var other = cmds.SingleOrDefault(x => x.Name == cmd.OpinionDescription);
            if (other == null)
            {
                Console.WriteLine($"Unable to find for opinion2 {cmd.OpinionDescription}");
                continue;
            }

            cmd.OpinionScore = other.OpinionScore;
            cmd.OpinionDescription = other.OpinionDescription;
        }

        return cmds;
    }

    private Command CreateNewCommand(
        CommandType commandType,
        string name,
        string description,
        int memoryConsumption,
        int opinionScore,
        string opinionDesc,
        bool shop,
        int? cost)
    {
        var cmd = new Command()
        {
            Name = name,
            CommandType = commandType,
            Description = description,
            MemoryConsumption = memoryConsumption,
            OpinionScore = opinionScore,
            OpinionDescription = opinionDesc,
            Shop = shop,
            Cost = cost
        };
        return cmd;
    }
}
