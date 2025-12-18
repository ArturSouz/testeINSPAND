using System.Linq;
using System.Reflection;
using Domain.Interfaces;
using Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FluentValidation.Results;

using Domain.Entities;

namespace Infrastructure.Data;

public class SqlDbContext : DbContext
{
    private readonly IDomainEventHandler _domainEventService;

    public SqlDbContext(DbContextOptions<SqlDbContext> options, IDomainEventHandler domainEventService) : base(options)
        => _domainEventService = domainEventService;

    public DbSet<Book> Books { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchEvents();

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primeiro, ignora propriedades problemáticas antes de aplicar outras configurações
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            var clrType = entityType.ClrType;
            // Verifica se a entidade herda de Entity<T>
            var baseType = clrType.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Entity<>))
                {
                    var propertiesToRemove = entityType.GetProperties()
                        .Where(p => p.ClrType == typeof(ValidationResult) || 
                                   (p.ClrType.IsGenericType && 
                                    p.ClrType.GetGenericTypeDefinition() == typeof(List<>) && 
                                    p.ClrType.GetGenericArguments().Length > 0 &&
                                    typeof(DomainEvent).IsAssignableFrom(p.ClrType.GetGenericArguments()[0])))
                        .ToList();
                    
                    foreach (var property in propertiesToRemove)
                    {
                        entityType.RemoveProperty(property);
                    }
                    break;
                }
                baseType = baseType.BaseType;
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(dateTimeConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private async Task DispatchEvents()
    {
        while (true)
        {
            try
            {
                var domainEventEntity = ChangeTracker.Entries<IHasDomainEvent>()
                    .Select(x => x.Entity.DomainEvents)
                    .SelectMany(x => x)
                    .Where(domainEvent => !domainEvent.IsPublished)
                    .FirstOrDefault();

                if (domainEventEntity is null) break;

                domainEventEntity.IsPublished = true;

                await _domainEventService.Publish(domainEventEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}