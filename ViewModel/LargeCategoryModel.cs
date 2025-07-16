using FluentValidation;
using PhrazorApp.Common;
using PhrazorApp.Models;
using PhrazorApp.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace PhrazorApp.ViewModel
{

    public class LargeCategoryModel
    {
        [Display(Name = "ジャンルId")]
        public Guid Id { get; set; }

        [Display(Name = "ジャンル名")]
        public string Name { get; set; } = string.Empty;

        public List<SmallCategoryModel> SubCategories { get; set; } = new();


    }

    public class SmallCategoryModel
    {

        [Required(ErrorMessage = ComMessage.REQUIRED_DETAIL)]
        [Display(Name = "サブジャンルId")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = ComMessage.REQUIRED_DETAIL)]
        [Display(Name = "サブジャンル名")]
        public string Name { get; set; } = string.Empty;


        public bool IsEditing { get; set; }

    }


}

public class LargeCategoryModelValidator : AbstractValidator<LargeCategoryModel>
{
    public LargeCategoryModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithName("カテゴリId")
            .WithMessage(string.Format(ComMessage.REQUIRED_DETAIL, "カテゴリId"));
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(string.Format(ComMessage.REQUIRED_DETAIL, "カテゴリ名"));

        RuleForEach(x => x.SubCategories)
            .SetValidator(new SmallCategoryModelValidator());

    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<LargeCategoryModel>.CreateWithOptions((LargeCategoryModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };

}
public class SmallCategoryModelValidator : AbstractValidator<SmallCategoryModel>
{
    public SmallCategoryModelValidator()
    {
        RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithName("サブカテゴリId")
                    .WithMessage("{PropertyName}を入力してください。");
        RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithName("サブカテゴリ名")
                    .WithMessage("{PropertyName}を入力してください。");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<SmallCategoryModel>.CreateWithOptions((SmallCategoryModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };


}
