using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components.Forms;
using PhrazorApp.UI.State;

namespace PhrazorApp.Extensions
{
    public static class EditContextExtensions
    {
        public static void PublishPageLevelErrors(
            this EditContext editContext,
            IServiceProvider services)
        {
            // ページメッセージをクリア
            var pageMessageStore = services.GetRequiredService<PageMessageStore>();
            pageMessageStore.Clear();

            var modelType = editContext.Model.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
            var validatorObj = services.GetService(validatorType);
            if (validatorObj is null) return;

            var context = new ValidationContext<object>(editContext.Model!);
            var result = ((IValidator)validatorObj).Validate(context);

            var pageOnlyMessages = result.Errors
                .Where(e => string.IsNullOrEmpty(e.PropertyName) &&
                            !string.IsNullOrWhiteSpace(e.ErrorMessage))
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            if (pageOnlyMessages.Count > 0)
                pageMessageStore.Errors(pageOnlyMessages);
        }
    }

}
