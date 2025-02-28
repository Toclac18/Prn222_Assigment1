using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsManagementSystem.Models
{
    public partial class SystemAccount
    {
        public short AccountId { get; set; }

        public string? AccountName { get; set; }

        public string? AccountEmail { get; set; }

        public int? AccountRole { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string? AccountPassword { get; set; }

        public bool IsActive { get; set; } = false;

        public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
    }
}
