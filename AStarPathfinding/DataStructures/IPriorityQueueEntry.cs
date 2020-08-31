using System;

namespace Pathfinding.DataStructures
{
    public interface IPriorityQueueEntry<TItem>
    {
        TItem Item { get; }
    }
}
