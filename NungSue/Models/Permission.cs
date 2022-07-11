namespace NungSue.Models
{
    public class PermissionField
    {
        public PermissionField(string value, string description)
        {
            Value = value;
            Description = description;
        }

        public string Value { get; set; }
        public string Description { get; set; }
    }
}
