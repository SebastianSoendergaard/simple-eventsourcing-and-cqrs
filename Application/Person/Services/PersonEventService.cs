using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Person.Projections;
using Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Person.Services
{
    public class PersonEventService : IHostedService
    {
        private readonly IServiceProvider services;
        private readonly UniquePersonProjection uniquePersonProjection;

        public PersonEventService(IServiceProvider services, UniquePersonProjection uniquePersonProjection)
        {
            this.services = services;
            this.uniquePersonProjection = uniquePersonProjection;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.uniquePersonProjection.UniquePersonConstraintBroken += OnUniquePersonConstraintBroken;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.uniquePersonProjection.UniquePersonConstraintBroken -= OnUniquePersonConstraintBroken;
            return Task.CompletedTask;
        }

        private void OnUniquePersonConstraintBroken(object sender, string personId)
        {
            // We created a person with an existing name, thus broke the contraint.
            // This can happen as the system is only eventually consistent and someone else has created a person 
            // in the time between check in this service and update of the projection.

            // Create a compensating event
            var service = services.GetRequiredService<IPersonService>();
            try
            {
                service.DeletePerson(new Core.Person.PersonId(personId), "Person already exist!");
            }
            catch (NotFoundException)
            { 
                // It is ok, person might already have been deleted
            }
        }
    }
}
