namespace NungSue.ViewModels;

public class AddressViewModel
{
    public Guid AddressId { get; set; }
    public string FullName { get; set; }
    public string FullAddress { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsDefault { get; set; }
}
