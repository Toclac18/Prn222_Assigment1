using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsManagementSystem.Models
{
    public partial class NewsArticle
    {
        [Key]
        public string NewsArticleId { get; set; } = null!;

        [Required(ErrorMessage = "News Title không được để trống")]
        [StringLength(200, ErrorMessage = "News Title không được quá 200 ký tự")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline không được để trống")]
        [StringLength(500, ErrorMessage = "Headline không được quá 500 ký tự")]
        public string Headline { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
        public string? NewsContent { get; set; }

        public string? NewsSource { get; set; }

        [Required(ErrorMessage = "Category không được để trống")]
        public short? CategoryId { get; set; }

        [Required(ErrorMessage = "News Status không được để trống")]
        public bool? NewsStatus { get; set; }

        public short? CreatedById { get; set; }

        public short? UpdatedById { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual Category? Category { get; set; }

        public virtual SystemAccount? CreatedBy { get; set; }

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
