using NungSue.Models;

namespace NungSue.ViewModels
{
    public class CategoryTagViewModel
    {
        public string Name { get; set; }
        public string TotalRecord { get; set; }
        public string Type { get; set; }
        public List<BookItem> BookItems { get; set; }
    }
}