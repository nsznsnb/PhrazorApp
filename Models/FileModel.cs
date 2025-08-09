using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;

namespace PhrazorApp.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public IBrowserFile File { get; set; }
    }


}
