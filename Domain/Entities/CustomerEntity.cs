using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CustomerEntity
{
    public int CustomerId { get; set; }

    public string? Name { get; set; }
    public bool State { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<PostEntity> Posts { get; set; } = new List<PostEntity>();
}
