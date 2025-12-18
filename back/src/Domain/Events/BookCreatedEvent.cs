using Domain.Entities;

namespace Domain.Events;

public class BookCreatedEvent : DomainEvent
{
    public Book Book { get; }

    public BookCreatedEvent(Book book)
    {
        Book = book;
    }
}

