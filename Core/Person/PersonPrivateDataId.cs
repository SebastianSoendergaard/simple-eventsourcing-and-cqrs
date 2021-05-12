using System;
using Framework.DDD;

namespace Core.Person
{
    public class PersonPrivateDataId : EntityId
    {
        private Guid _guid;

        public PersonPrivateDataId()
        {
            _guid = Guid.NewGuid();
        }

        public PersonPrivateDataId(string id)
        {
            _guid = Guid.Parse(id);
        }

        public override string ToString() => _guid.ToString();
    }
}
