using System.Xml.Linq;

namespace MVCLibrary.Models
{
    public class Book
    {
        public int ID { get; set; }
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public string Edition { get; set; } = "";
        public int NumberPages { get; set; }

        public string? Contents { get; set; }

        public List<Autor> Autors { get; set; } = new();

        public string AuthorsDisplay =>
            Autors.Count == 0 ? "—" : string.Join(", ", Autors.Select(a => a.FullName));

        public List<string> ContentsItems
        {
            get
            {
                var result = new List<string>();

                if (string.IsNullOrWhiteSpace(Contents))
                    return result;

                try
                {
                    var doc = XDocument.Parse(Contents);

                    var items = doc
                        .Descendants()
                        .Where(x => !x.HasElements)
                        .Select(x => x.Value?.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToList();

                    result.AddRange(items);
                }
                catch
                {
                    result.Add(Contents.Trim());
                }

                return result;
            }
        }
    }
}
