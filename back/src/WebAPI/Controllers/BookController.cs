using Domain.Entities;
using Domain.Events;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class BookController : ControllerBase
{
    private readonly SqlDbContext _context;

    public BookController(SqlDbContext context)
    {
        _context = context;
    }

    [HttpGet()]
    public async Task<IActionResult> Get(uint page = 1, uint pageSize = 10)
    {
        var totalCount = await _context.Books.CountAsync();
        var books = await _context.Books
            .OrderBy(b => b.Id)
            .Skip((int)(page - 1) * (int)pageSize)
            .Take((int)pageSize)
            .Select(b => new { b.Id, b.Title, b.Author, b.Description })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new 
        { 
            books, 
            totalCount, 
            totalPages, 
            currentPage = page,
            pageSize 
        });
    }

    [HttpPost()]
    public async Task<IActionResult> Create([FromBody] BookDto bookDto)
    {
        var book = new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            Description = bookDto.Description ?? string.Empty
        };

        if (!book.IsValid())
        {
            var errors = book.ValidationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(new { error = string.Join(" ", errors) });
        }

        // Verifica título duplicado
        var existingBook = await _context.Books
            .FirstOrDefaultAsync(b => b.Title.ToLower() == book.Title.ToLower());

        if (existingBook != null)
        {
            return BadRequest(new { error = "Já existe um livro com este título." });
        }

        book.SetLastAction();
        await _context.Books.AddAsync(book);
        
        // Dispara evento de domínio quando um livro é criado
        book.DomainEvents.Add(new BookCreatedEvent(book));
        
        await _context.SaveChangesAsync();

        return Ok(new { book.Id, book.Title, book.Author, book.Description });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] BookDto bookDto)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { error = "Livro não encontrado." });
        }

        book.Title = bookDto.Title;
        book.Author = bookDto.Author;
        book.Description = bookDto.Description ?? string.Empty;

        if (!book.IsValid())
        {
            var errors = book.ValidationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(new { error = string.Join(" ", errors) });
        }

        // Verifica título duplicado (excluindo o livro atual)
        var existingBook = await _context.Books
            .FirstOrDefaultAsync(b => b.Title.ToLower() == book.Title.ToLower() && b.Id != id);

        if (existingBook != null)
        {
            return BadRequest(new { error = "Já existe um livro com este título." });
        }

        book.SetLastAction();
        await _context.SaveChangesAsync();

        return Ok(new { book.Id, book.Title, book.Author, book.Description });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { error = "Livro não encontrado." });
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class BookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Description { get; set; }
}

