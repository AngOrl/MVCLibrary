using MVCLibrary.Models;

namespace MVCLibrary.Data
{
    public interface ILibraryRepository
    {
        List<Book> GetAllBooks();
        List<Autor> GetAllAutors();

        int InsertAutor(string firstName, string surname);
        int InsertBook(BookCreateVm vm);

        void DeleteBook(int id);
        void DeleteAutor(int id);

        void UpdateBook(int id, string title, int year, string edition, int pages, string? contentsXml);
        void UpdateAutor(AutorEditVm vm);

        List<int> GetAutorIdsByBook(int bookId);      
        void AddAutorToBook(int bookId, int autorId);  
        void RemoveAutorFromBook(int bookId, int autorId);
    }
}
