using Domain.Events;
using Domain.Interfaces;
using MediatR;

namespace Infrastructure.Events;

public class BookCreatedEventHandler : INotificationHandler<DomainEventNotification<BookCreatedEvent>>
{
    private readonly IEmailService _emailService;

    public BookCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public Task Handle(DomainEventNotification<BookCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var book = notification.DomainEvent.Book;
        var message = $"Um novo livro foi criado:\n\n" +
                     $"Título: {book.Title}\n" +
                     $"Autor: {book.Author}\n" +
                     $"Descrição: {book.Description}\n" +
                     $"ID: {book.Id}\n" +
                     $"Data de criação: {book.CreatedDate:dd/MM/yyyy HH:mm:ss}";

        _emailService.SendEmail("developers@inspand.com.br", message);

        return Task.CompletedTask;
    }
}

