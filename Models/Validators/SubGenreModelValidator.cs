using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public class SubGenreModelValidator : AbstractValidator<SubGenreModel>
    {
        public SubGenreModelValidator()
        {
            RuleFor(x => x.Id)
                        .NotEmpty()
                        .WithName("サブジャンルId")
                        .WithMessage("{PropertyName}を入力してください。");
            RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithName("サブジャンル名")
                        .WithMessage("{PropertyName}を入力してください。");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<SubGenreModel>.CreateWithOptions((SubGenreModel)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };


    }
}
