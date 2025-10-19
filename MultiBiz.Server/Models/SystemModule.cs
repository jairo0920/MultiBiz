using MultiBiz.Shared.Modules;

namespace MultiBiz.Server.Models;
public class SystemModule : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public ModuleType Type { get; set; }
    public bool IsEnabled { get; set; } = true;
}
