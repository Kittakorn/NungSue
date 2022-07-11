
namespace NungSue.Areas.Admin.ViewModels.Book
{
    public class BookDetailsViewModel
    {
        public Guid BookId { get; set; }
        public string ImageUrl { get; set; }
        public string Barcode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string ListOfContent { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public string NumberOfPage { get; set; }
        public string MonthOfPublication { get; set; }
        public string Price { get; set; }
        public string PublishedOn { get; set; }
        public string CreateBy { get; set; }
        public string CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDate { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public string IsPublish { get; set; }
        public string Tags { get; set; }
        public string Authors { get; set; }
        public string NewPrice { get; set; }
        public string PromotionText { get; set; }

    }
}