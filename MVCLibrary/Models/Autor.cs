namespace MVCLibrary.Models
{
    public class Autor
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";

        public string FullName => $"{Surname} {FirstName}";
    }
}
