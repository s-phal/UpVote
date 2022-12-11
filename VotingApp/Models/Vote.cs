namespace VotingApp.Models
{
	public class Vote
	{
		public int Id { get; set; }

		public int IdeaId { get; set; }
		public virtual Idea? Idea { get; set; }

		public string MemberId { get; set; }
		public virtual Member? Member { get; set; }


	}
}
