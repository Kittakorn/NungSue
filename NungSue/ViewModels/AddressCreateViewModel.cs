namespace NungSue.ViewModels;

public class AddressCreateViewModel
{
    public Guid AddressId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string District { get; set; }
    public string SubDistrict { get; set; }
    public string Province { get; set; }
    public string ZipCode { get; set; }
    public bool IsDefault { get; set; }
}
