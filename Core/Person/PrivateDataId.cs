using System;
using Framework.DDD;

namespace Core.Person
{
    public class PrivateDataId : EntityId
    {
        private Guid _guid;

        public PrivateDataId()
        {
            _guid = Guid.NewGuid();
        }

        public PrivateDataId(string id)
        {
            _guid = Guid.Parse(id);
        }

        public override string ToString() => _guid.ToString();
    }
}
