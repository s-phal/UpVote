﻿using System.ComponentModel.DataAnnotations;

namespace VotingApp.Models
{
    public class Idea
    {
        public int Id { get; set; }
        public string Title { get; set; }

        //[StringLength(80, ErrorMessage = "Description must be at least 15 characters and can not exceed 80 characters.", MinimumLength = 15)]
        public string Description { get; set; }

        public string CurrentStatus { get; set; } = "open";

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public bool IsModerated { get; set; } = false;

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string MemberId { get; set; }
        public virtual Member? Member { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<Vote> Votes { get; set; } = new HashSet<Vote>();        

    }
}
