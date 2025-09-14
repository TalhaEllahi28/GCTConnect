using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Post
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public string? MediaUrl { get; set; }

    public string? MediaType { get; set; }

    public string Privacy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int LikesCount { get; set; }

    public int CommentsCount { get; set; }

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual User User { get; set; } = null!;
}
