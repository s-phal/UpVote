using System.ComponentModel.DataAnnotations;

namespace VotingApp.Models
{
    public class Idea
    {
        public int Id { get; set; }
        public string Title { get; set; }

        //[StringLength(80, ErrorMessage = "Description must be at least 15 characters and can not exceed 80 characters.", MinimumLength = 15)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string MemberId { get; set; }
        public virtual Member? Member { get; set; }



    }
}
