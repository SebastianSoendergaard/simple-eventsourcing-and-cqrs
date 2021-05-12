using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Person.Projections;
using Application.Person.Services;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Model;
using Swashbuckle.Swagger.Annotations;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {

        private readonly IPersonService _personService;
        private readonly PersonListProjection _personListProjection;

        public PersonController(IPersonService personService, PersonListProjection personListProjection)
        {
            _personService = personService;
            _personListProjection = personListProjection;
        }
 
        /// <summary>
        /// Creates new person Aggregate using first and last name as parameters
        /// Person will be saved as stream of events in event store
        /// </summary>
        /// <param name="person"></param>
        /// <returns>Newly created person object aggregateId</returns>
        [HttpPost]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(object))]
        public async Task<object> CreatePerson([FromBody]CreatePersonDTO person)
        {
            var insertedPersonId = await _personService.CreatePerson(person.FirstName, person.LastName);
            return new { PersonId = insertedPersonId.ToString() };
        }

        /// <summary>
        /// Returns a list of persons, reply should be very quick as the values are precalculated in a projection.
        /// </summary>
        /// <returns>A list of persons</returns>
        [HttpGet]
        [Route("allPersons")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(ListPersonDto))]
        public IEnumerable<ListPersonDto> GetAllPersons()
        {
            var personList = _personListProjection.GetPersonList();
            return personList.Select(p => new ListPersonDto 
            { 
                PersonId = p.PersonId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                PhoneNumber = p.PhoneNumber,
                IdDeleted = p.IsDeleted,
                DeleteReason = p.DeleteReason
            });
        }

        /// <summary>
        /// Fetch aggregate from event store using aggregateId(personId)
        /// This will fetch all the events for given aggregate and mutate
        /// aggregate using each event in sequence, therefore reconstructing
        /// latest aggregate state
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(PersonDto))]
        public async Task<PersonDto> GetPerson([FromQuery]string personId)
        {
            var person = await _personService.GetPerson(new Core.Person.PersonId(personId));
            return new PersonDto
            { 
                PersonId = person.Id.ToString(),
                FirstName = person.FirstName,
                LastName = person.LastName,
                Address = person.PersonAddress != null ? new AddressDto
                { 
                    Street = person.PersonAddress.Street,
                    City = person.PersonAddress?.City,
                    ZipCode = person.PersonAddress.ZipCode,
                    Country = person.PersonAddress.Country
                }
                : null,
                PhoneNumber = person.PhoneNumber,
                IdDeleted = person.IsDeleted               
            };
        }

        /// <summary>
        /// Updates person address. New address is added as an event in event store
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updateAddress")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        public async Task UpdatePersonAddress(
            [FromQuery]string personId,
            [FromBody]AddressDto address)
        {
            await _personService.UpdatePersonAddress(new Core.Person.PersonId(personId),
                address.City, address.Country, address.Street, address.ZipCode);
            Ok();
        }

        /// <summary>
        /// Updates person phone number. New phone number is added as an event in event store
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updatePhoneNumber")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        public async Task UpdatePhoneNumber(
            [FromQuery] string personId,
            [FromForm] string phoneNumber)
        {
            await _personService.UpdatePhoneNumber(new Core.Person.PersonId(personId), phoneNumber);
            Ok();
        }

        /// <summary>
        /// Updates person address. New address is added as an event in event store
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deletePerson")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        public async Task DeletePerson([FromQuery] string personId, [FromQuery] string reason)
        {
            await _personService.DeletePerson(new Core.Person.PersonId(personId), reason);
            Ok();
        }
    }
}
