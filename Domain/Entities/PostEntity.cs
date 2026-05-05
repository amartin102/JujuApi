using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class PostEntity
{
    public int PostId { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public int Type { get; set; }

    public string? Category { get; set; }

    public int CustomerId { get; set; }

    public bool State { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual CustomerEntity Customer { get; set; } = null!;
}
