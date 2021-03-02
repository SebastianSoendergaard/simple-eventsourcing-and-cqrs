using System.Threading.Tasks;
using Core.Person;
using RestAPI.Model;

namespace RestAPI.Services
{
    public interface IPersonService
    {
        Task<PersonId> CreatePerson(string firstName, string lastName);
        Task<PersonDto> GetPerson(PersonId personId);

        Task UpdatePersonAddress(PersonId personId, string city, string country, string street, string zipcode);

        Task DeletePerson(PersonId personId, string reason);
    }
}
