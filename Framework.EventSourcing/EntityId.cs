// This file was originally taken from https://github.com/aneshas/tactical-ddd

using System.Collections.Generic;

namespace Framework.DDD
{
    public abstract class EntityId : ValueObject, IEntityId
    {
        public abstract override string ToString();

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ToString();
        }
    }
}