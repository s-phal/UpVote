using System.ComponentModel.DataAnnotations;

namespace VotingApp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public int IdeadId { get; set; }
        public virtual Idea? Idea { get; set; }

        public string MemberId { get; set; }
        public virtual Member? Member { get; set; }

        public int CommentId { get; set; }
        public virtual Comment? Comment { get; set; }

    }
}
