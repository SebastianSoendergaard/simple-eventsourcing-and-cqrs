using System.Threading.Tasks;
using Core.Person;
using EventStoreTests.Infrastructure;
using Framework.DDD.EventStore;
using Infrastructure.EventStore;
using Infrastructure.Factories;
using NUnit.Framework;

namespace EventStoreTests.Integration
{
    [TestFixture]
    public class EventStoreIntegrationTests : IntegrationTestBase
    {

        private IEventStore _eventStore;

        [SetUp]
        public void SetUp()
        {
            _eventStore = new EventStoreRepository(new SqlConnectionFactory(ConnectionString));
        }

        [Test]
        public async Task Given_PersonAggregateCreated_When_SavedToEventStore_Then_ShouldBeTheSameWhenFetched()
        {
            var personId = new PersonId();

            var personAggregate = Person.CreateNewPerson("Chuck", "Norris");

            await _eventStore.SaveAsync(personId, personAggregate.Version, personAggregate.DomainEvents, "PersonAggregate");
            
            var results = await _eventStore.LoadAsync(personId);

            var fetchedPerson = new Person(results);
            Assert.IsNotNull(results);
            Assert.AreEqual(personAggregate, fetchedPerson);
        }

        [Test]
        public async Task Given_PersonAggregate_When_AddressChanged_Then_ShouldBeTheSameWhenFetched()
        {
            var personId = new PersonId();

            var personAggregate = Person.CreateNewPerson("Chuck", "Norris");
            personAggregate.ChangePersonAddress("street1", "country1", "111222", "city");

            await _eventStore.SaveAsync(personId, personAggregate.Version, personAggregate.DomainEvents, "PersonAggregate");

            var results = await _eventStore.LoadAsync(personId);

            var fetchedPerson = new Person(results);
            Assert.IsNotNull(results);
            Assert.AreEqual(fetchedPerson.PersonAddress.City, "city");
            Assert.AreEqual(fetchedPerson.PersonAddress.Country, "country1");
            Assert.AreEqual(fetchedPerson.PersonAddress.ZipCode, "111222");
            Assert.AreEqual(fetchedPerson.PersonAddress.Street, "street1");
        }
    }
}
