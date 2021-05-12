// This file was originally taken from https://github.com/aneshas/tactical-ddd

using System;

namespace Framework.DDD
{
    public interface IDomainEvent
    {
        DateTime CreatedAt { get; set; }
    }
}