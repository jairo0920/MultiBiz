namespace MultiBiz.Shared.Barbershop;
public record BarberDto(Guid Id, string Name, bool IsActive, Guid TenantId);
public record ServiceDto(Guid Id, string Name, int Minutes, decimal Price, Guid TenantId);
public record AppointmentDto(Guid Id, Guid BarberId, Guid ServiceId, DateTime Start, string CustomerName, string CustomerPhone, bool Confirmed, Guid TenantId);
public record CreateAppointmentRequest(Guid BarberId, Guid ServiceId, DateTime Start, string CustomerName, string CustomerPhone);
