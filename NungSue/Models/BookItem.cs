namespace NungSue.Models
{
    public class BookItem
    {
        public Guid BookId { get; set; }
        public string Barcode{ get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string PromotionPrice { get; set; }
        public string PromotionText { get; set; }
        public string BookImageUrl { get; set; }
        public bool IsFavorite { get; set; }
    }
}
