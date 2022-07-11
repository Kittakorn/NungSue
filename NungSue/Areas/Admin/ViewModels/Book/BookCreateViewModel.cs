using NungSue.Entities;
using System.ComponentModel.DataAnnotations;
using static NungSue.Helpers.CustomValidation;

namespace NungSue.Areas.Admin.ViewModels.Book
{
    public class BookCreateViewModel : IValidatableObject
    {
        public Guid BookId { get; set; }

        [MaxLength(13)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [RegularExpression(@"^\d{13}", ErrorMessage = "บาร์โค้ดไม่ถูกต้อง")]
        public string Barcode { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string Title { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public string Content { get; set; }

        public string ListOfContents { get; set; }

        [RequiredBookImageIfIdNull(ErrorMessage = "กรุณาเลือกภาพปก")]
        public IFormFile BookImage { get; set; }

        [MaxLength(6)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [RegularExpression(@"^(?!0+\.00)(?=.{1,9}(\.|$))(?!0(?!\.))\d{1,3}(,\d{3})*(\.\d+)?$", ErrorMessage = "ราคาไม่ถูกต้อง")]
        public string Price { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [RegularExpression(@"^[1-9][0-9][0-9][ ][X][ ][1-9][0-9][0-9][ ][X][ ]([1-9]|[1-9][0-9])$", ErrorMessage = "ขนาดไม่ถูกต้อง")]
        public string Size { get; set; }

        [MaxLength(5)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [RegularExpression(@"^(?!0+\.00)(?=.{1,9}(\.|$))(?!0(?!\.))\d{1,3}(,\d{3})*(\.\d+)?$", ErrorMessage = "น้ำหนักไม่ถูกต้อง")]
        public string Weight { get; set; }

        [MaxLength(5)]
        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        [RegularExpression(@"^(?!0+\.00)(?=.{1,9}(\.|$))(?!0(?!\.))\d{1,3}(,\d{3})*(\.\d+)?$", ErrorMessage = "จำนวนหน้าไม่ถูกต้อง")]
        public string NumberOfPage { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public DateTime MonthOfPublication { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public DateTime PublishedOn { get; set; } = DateTime.Now.Date.AddHours(12);

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public List<Guid> TagIds { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public List<Guid> AuthorIds { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกข้อมูล")]
        public Guid PublisherId { get; set; }

        public bool IsPublish { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = validationContext.GetService<BookStoreContext>();
            var results = new List<ValidationResult>();

            if (context.Books.Any(x => x.Barcode == Barcode && x.BookId != BookId))
                results.Add(new ValidationResult("มีบาร์โค้ดนี้ในระบบแล้ว", new[] { nameof(Barcode) }));

            var data = context.Books.Any(x => x.Title == Title && x.BookId != BookId);
            if (context.Books.Any(x => x.Title == Title && x.BookId != BookId))
                results.Add(new ValidationResult("มีชื่อหนังสือนี้ในระบบแล้ว", new[] { nameof(Title) }));

            return results;
        }
    }
}