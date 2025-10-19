namespace MultiBiz.Server.Models;
public class Barber : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Service : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public int Minutes { get; set; }
    public decimal Price { get; set; }
}

public class Appointment : BaseTenantEntity
{
    public Guid BarberId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime Start { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public bool Confirmed { get; set; }
}
