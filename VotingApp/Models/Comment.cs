﻿using System.ComponentModel.DataAnnotations;

namespace VotingApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [StringLength(300, ErrorMessage = "Comment must be at least 15 characters and can not exceed 400 characters.", MinimumLength = 5)]
        public string Body { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public int SpamReports { get; set; } = 0;

        public bool IsModerated { get; set; } = false;


        public string MemberId { get; set; }
        public Member? Member { get; set; }

        public int IdeaId { get; set; }
        public Idea? Idea { get; set; }
    }
}
