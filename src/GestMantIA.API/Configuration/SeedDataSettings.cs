namespace GestMantIA.API.Configuration;

public class SeedDataSettings
{
    public UserSeedData AdminUser { get; set; } = new();
    public UserSeedData RegularUser { get; set; } = new();
    public SampleDataSettings SampleData { get; set; } = new();
}

public class UserSeedData
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SampleDataSettings
{
    public bool Enable { get; set; }
    public int NumSampleUsers { get; set; }
    public int NumSampleClients { get; set; }
    public int NumSampleWorkOrders { get; set; }
}
