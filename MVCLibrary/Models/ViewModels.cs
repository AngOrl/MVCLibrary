using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCLibrary.Models
{
    public class BookCreateVm
    {

        public string Title { get; set; } = "";
        public int Year { get; set; }
        public string Edition { get; set; } = "";
        public int NumberPages { get; set; }
        public string? Contents { get; set; }
        public List<int> SelectedAutorIds { get; set; } = new();
        public List<SelectListItem> AllAutors { get; set; } = new();
    }

    public class BookEditVm : BookCreateVm
    {
        public int ID { get; set; }
    }

    public class AutorEditVm
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
    }

    public class AutorCreateVm
    {
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
    }

}
