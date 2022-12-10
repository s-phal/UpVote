namespace VotingApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Idea> Ideas { get; set; } = new HashSet<Idea>();
    }
}
