using System.Threading.Tasks;
using Application.Person.Projections;
using Core.Exceptions;
using Core.Person;
using Core.Person.Repositories;

namespace Application.Person.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly UniquePersonProjection _uniquePersonProjection;

        public PersonService(IPersonRepository personRepository, UniquePersonProjection uniquePersonProjection)
        {
            _personRepository = personRepository;
            _uniquePersonProjection = uniquePersonProjection;
            _uniquePersonProjection.UniquePersonConstraintBroken += OnUniquePersonConstraintBroken;
        }

        private void OnUniquePersonConstraintBroken(object sender, string personId)
        {
            // We created a person with an existing name, thus broke the contraint.
            // This can happen as the system is only eventually consistent and someone else has created a person 
            // in the time between check in this service and update of the projection.

            // Create a compensating event
            _ = DeletePerson(new PersonId(personId), "Person already exist!"); 
        }

        public async Task<PersonId> CreatePerson(string firstName, string lastName)
        {
            if(!_uniquePersonProjection.IsPersonUnique(firstName, lastName))
            {
                throw new ValidationException("firstName and lastName must be unique!");
            }

            var person = Core.Person.Person.CreateNewPerson(firstName, lastName);
            var pid =  await _personRepository.SavePersonAsync(person);
            return pid;
        }

        public async Task<Core.Person.Person> GetPerson(PersonId personId)
        {
            return await GetPersonOrThrow(personId);
        }


        public async Task UpdatePersonAddress(PersonId personId, string city, string country, string street, string zipcode)
        {
            var person = await GetPersonOrThrow(personId);
            person.ChangePersonAddress(street, country, zipcode, city);
            await _personRepository.SavePersonAsync(person);
        }

        public async Task UpdatePhoneNumber(PersonId personId, string phoneNumber)
        {
            var person = await GetPersonOrThrow(personId);
            person.ChangePhoneNumber(phoneNumber);
            await _personRepository.SavePersonAsync(person);
        }

        public async Task DeletePerson(PersonId personId, string reason)
        {
            var person = await GetPersonOrThrow(personId);
            person.DeletePerson(reason);
            await _personRepository.SavePersonAsync(person);
        }

        private async Task<Core.Person.Person> GetPersonOrThrow(PersonId personId)
        {
            var person = await _personRepository.GetPerson(personId);

            if (person == null) throw new NotFoundException($"Person with id '{personId}' was not found");

            return person;
        }
    }
}
