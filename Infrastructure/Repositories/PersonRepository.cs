using System.Threading.Tasks;
using Core.Person;
using Core.Person.Repositories;
using Framework.DDD.EventStore;

namespace Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IEventStore _eventStore;
        public PersonRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Person> GetPerson(PersonId id)
        {
            var personEvents = await _eventStore.LoadAsync(id);
            var person = new Person(personEvents);
            if (person.PersonPrivateDataId != null)
            {
                var privateEvents = await _eventStore.LoadAsync(person.PersonPrivateDataId);
                person.ApplyPrivateDataEvents(privateEvents);
            }
            return person;
        }

        public async Task<PersonId> SavePersonAsync(Person person)
        {
            await _eventStore.SaveAsync(person.Id, person.Version, person.DomainEvents, person.GetType().Name);

            var privateData = person.GetPrivateData();
            if (privateData != null)
            {
                await _eventStore.SaveAsync(privateData.Id, privateData.Version, privateData.DomainEvents, privateData.GetType().Name);
            }
            else
            {
                // Comply with GDPR by deleting private data but keeping person instance to avoid breaking references and that way ensure system will not break
                // From here all changes to person will try to delete private data, but that should be ok as person is deleted and we do not expect many changes
                await _eventStore.DeleteAsync(person.PersonPrivateDataId);
            }

            return person.Id;
        }
    }
}
