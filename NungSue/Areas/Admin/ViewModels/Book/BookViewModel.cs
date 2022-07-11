namespace NungSue.Areas.Admin.ViewModels.Book
{
    public class BookViewModel
    {
        public Guid BookId { get; set; }
        public string Barcode { get; set; }
        public string BookCategory { get; set; }
        public string Title { get; set; }
        public string NumStars { get; set; }
        public string Price { get; set; }
        public string PromotionPrice { get; set; }
        public string PublishedOn { get; set; }
        public string Tags { get; set; }
        public string Authors { get; set; }
        public bool IsPublish { get; set; }
    }
}