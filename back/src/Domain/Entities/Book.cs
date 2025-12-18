using FluentValidation;

namespace Domain.Entities;

public class Book : Entity<Book>
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public override bool IsValid()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .Length(10, 100).WithMessage("Título deve ter entre 10 e 100 caracteres.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Autor é obrigatório.")
            .Length(10, 100).WithMessage("Autor deve ter entre 10 e 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1024).WithMessage("Descrição deve ter no máximo 1024 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        ValidationResult = Validate(this);
        return ValidationResult.IsValid;
    }
}

