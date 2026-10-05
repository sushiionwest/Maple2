using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Database.Mods;
using Maple2.Model.Enum;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;

namespace Maple2.Server.Game.Commands;

public class ReloadCommand : GameCommand {
    private readonly GameSession session;
    private readonly MetadataReloader reloader;

    public ReloadCommand(GameSession session, MetadataReloader reloader) : base(AdminPermissions.Debug, "reload", "Reload game metadata without restarting.") {
        this.session = session;
        this.reloader = reloader;

        var targets = new Argument<string[]>("targets", () => [], $"What to reload (default all): {string.Join(", ", reloader.Targets)}");
        var mods = new Option<bool>(["--mods", "-m"], "Apply the mods folder to the metadata database first.");
        var local = new Option<bool>(["--local", "-l"], "Only reload this channel instead of every channel.");

        AddArgument(targets);
        AddOption(mods);
        AddOption(local);
        this.SetHandler<InvocationContext, string[], bool, bool>(Handle, targets, mods, local);
    }

    private void Handle(InvocationContext ctx, string[] targets, bool mods, bool local) {
        string[] unknown = targets.Where(target => !target.Equals("all", StringComparison.OrdinalIgnoreCase)
                                                   && !reloader.Targets.Contains(target, StringComparer.OrdinalIgnoreCase)).ToArray();
        if (unknown.Length > 0) {
            ctx.Console.Error.WriteLine($"Unknown target(s): {string.Join(", ", unknown)}. Valid: all, {string.Join(", ", reloader.Targets)}");
            ctx.ExitCode = 1;
            return;
        }

        try {
            if (mods) {
                MetadataModApplier.Result result = reloader.ApplyMods();
                if (!result.Success) {
                    ctx.Console.Error.WriteLine(result.Describe());
                    ctx.ExitCode = 1;
                    return;
                }
                ctx.Console.Out.WriteLine(result.Describe());
            }

            if (local) {
                ctx.Console.Out.WriteLine($"Reloaded {string.Join(", ", reloader.Reload(targets))}");
                return;
            }

            var request = new AdminRequest {
                RequesterId = session.CharacterId,
                ReloadMetadata = new AdminRequest.Types.ReloadMetadata(),
            };
            request.ReloadMetadata.Targets.AddRange(targets);
            AdminResponse response = session.World.Admin(request);
            if (response.Error > 0) {
                ctx.Console.Error.WriteLine(response.Message);
                ctx.ExitCode = 1;
                return;
            }
            ctx.Console.Out.WriteLine(response.Message);
        } catch (Exception ex) {
            ctx.Console.Error.WriteLine($"Reload failed: {ex.Message}");
            ctx.ExitCode = 1;
        }
    }
}
