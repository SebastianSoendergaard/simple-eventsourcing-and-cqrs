using System.Threading.Tasks;
using Core.Person;

namespace Application.Person.Services
{
    public interface IPersonService
    {
        Task<PersonId> CreatePerson(string firstName, string lastName);
        Task<Core.Person.Person> GetPerson(PersonId personId);

        Task UpdatePersonAddress(PersonId personId, string city, string country, string street, string zipcode);

        Task UpdatePhoneNumber(PersonId personId, string phoneNumber);

        Task DeletePerson(PersonId personId, string reason);
    }
}
