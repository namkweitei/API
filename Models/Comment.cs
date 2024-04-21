using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public int ContributionId { get; set; } 
        public string UserId { get; set; } 
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsAnonymous { get; set; } 
        public bool Hidden { get; set; } 
        public int Likes { get; set; } 
        public int Dislikes { get; set; }
        public Contribution Contribution { get; set; }
        public List<LikeDislikeComment> LikeDislikeComments { get; set; }
        public List<CommentOfComment> CommentOfComments { get; set; }
    }
}
