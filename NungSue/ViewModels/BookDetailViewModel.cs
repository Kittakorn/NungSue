namespace NungSue.ViewModels
{
    public class BookDetailViewModel
    {
        public Guid BookId { get; set; }
        public string Barcode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string ListOfContents { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public string NumberOfPage { get; set; }
        public string MonthOfPublication { get; set; }
        public string Price { get; set; }
        public string NewPrice { get; set; }
        public string PromotionText { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Authors { get; set; }
        public string BookImage { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsPublish { get; set; }
        public string PublishedOn { get; set; }
    }
}
