using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;

namespace PhrazorApp.Models
{
    public class FileModel
    {
        public IBrowserFile File { get; set; } = default!;
    }


}
