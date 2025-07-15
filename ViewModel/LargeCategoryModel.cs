using FluentValidation;
using PhrazorApp.Common;
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

        public string ParentName { get; set; } = string.Empty;

        public bool IsEditing { get; set; }
    }


}

public class LargeCategoryModelValidator : AbstractValidator<LargeCategoryModel>
{
    public LargeCategoryModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ComMessage.REQUIRED_DETAIL);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ComMessage.REQUIRED_DETAIL);

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
            .NotEmpty().WithMessage(ComMessage.REQUIRED_DETAIL);
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ComMessage.REQUIRED_DETAIL);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<SmallCategoryModel>.CreateWithOptions((SmallCategoryModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
