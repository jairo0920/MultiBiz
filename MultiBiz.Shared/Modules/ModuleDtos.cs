namespace MultiBiz.Shared.Modules;
public enum ModuleType { Restaurant, Barbershop }
public record ModuleDto(Guid Id, string Name, ModuleType Type, Guid TenantId, bool IsEnabled);
public record CreateModuleRequest(string Name, ModuleType Type, Guid TenantId, bool IsEnabled);
