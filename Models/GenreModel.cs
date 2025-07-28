using FluentValidation;
using PhrazorApp.Constants;
using PhrazorApp.Models;
using System.ComponentModel.DataAnnotations;



namespace PhrazorApp.Models
{

    public class GenreModel
    {
        [Display(Name = "ジャンルId")]
        public Guid Id { get; set; }

        [Display(Name = "ジャンル名")]
        public string Name { get; set; } = string.Empty;

        public List<SubGenreModel>? SubGenres { get; set; } = new();


    }

    public class SubGenreModel
    {


        [Display(Name = "サブジャンルId")]
        public Guid Id { get; set; }

        [Display(Name = "サブジャンル名")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "ソート順")]
        public int SortOrder { get; set; }


    }


}

public class GenreModelValidator : AbstractValidator<GenreModel>
{
    public GenreModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithName("ジャンルId")
            .WithMessage(string.Format(ComMessage.MSG_E_REQUIRED_DETAIL, "ジャンルId"));
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(string.Format(ComMessage.MSG_E_REQUIRED_DETAIL, "ジャンル名"));

        RuleFor(x => x.SubGenres)
            .Must(x => x.Count <= 3)
            .WithMessage("サブジャンルの追加は3個までです。");

        RuleForEach(x => x.SubGenres)
            .SetValidator(new SubGenreModelValidator());

    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<GenreModel>.CreateWithOptions((GenreModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };

}

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
