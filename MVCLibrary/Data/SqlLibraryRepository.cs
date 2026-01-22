using System.Data;
using MVCLibrary.Models;
using Microsoft.Data.SqlClient;

namespace MVCLibrary.Data
{
    public class SqlLibraryRepository : ILibraryRepository
    {
        private readonly string _cs;

        public SqlLibraryRepository(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection")!;
        }

        public List<Autor> GetAllAutors()
        {
            var list = new List<Autor>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("SelectAllAutors", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Autor
                {
                    ID = Convert.ToInt32(reader["ID"]),
                    FirstName = reader["FirstName"].ToString()!,
                    Surname = reader["Surname"].ToString()!
                });
            }

            return list;
        }

        //SelectBooks
        public List<Book> GetAllBooks()
        {
            var dict = new Dictionary<int, Book>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("SelectBooks", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int bookId = Convert.ToInt32(reader["ID"]);

                if (!dict.TryGetValue(bookId, out var book))
                {
                    book = new Book
                    {
                        ID = bookId,
                        Title = reader["Title"].ToString()!,
                        Year = Convert.ToInt32(reader["Year"]),
                        Edition = reader["Edition"].ToString()!,
                        NumberPages = Convert.ToInt32(reader["NumberPages"]),
                        Contents = reader["Contents"] == DBNull.Value ? null : reader["Contents"].ToString()
                    };
                    dict[bookId] = book;
                }

                if (reader["AutorId"] != DBNull.Value)
                {
                    var autor = new Autor
                    {
                        ID = Convert.ToInt32(reader["AutorId"]),
                        FirstName = reader["FirstName"].ToString()!,
                        Surname = reader["Surname"].ToString()!
                    };

                    if (!book.Autors.Any(a => a.ID == autor.ID))
                        book.Autors.Add(autor);
                }
            }

            return dict.Values.ToList();
        }

        // InsertAutors
        public int InsertAutor(string firstName, string surname)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("InsertAutors", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@Surname", surname);

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        //  InsertBooks
        public int InsertBook(BookCreateVm vm)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("InsertBooks", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Year", vm.Year);
            cmd.Parameters.AddWithValue("@Title", vm.Title);
            cmd.Parameters.AddWithValue("@Edition", vm.Edition);
            cmd.Parameters.AddWithValue("@NumberPages", vm.NumberPages);

            cmd.Parameters.Add("@Contents", SqlDbType.Xml).Value =
                string.IsNullOrWhiteSpace(vm.Contents) ? DBNull.Value : vm.Contents;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // UpdateBooks
        public void UpdateBook(int id, string title, int year, string edition, int pages, string? contentsXml)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("UpdateBooks", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Year", year);
            cmd.Parameters.AddWithValue("@Edition", edition);
            cmd.Parameters.AddWithValue("@NumberPages", pages);

            cmd.Parameters.Add("@Contents", SqlDbType.Xml).Value =
                string.IsNullOrWhiteSpace(contentsXml) ? DBNull.Value : contentsXml;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // UpdateAutor
        public void UpdateAutor(AutorEditVm vm)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("UpdateAutors", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID", vm.ID);
            cmd.Parameters.AddWithValue("@FirstName", vm.FirstName);
            cmd.Parameters.AddWithValue("@Surname", vm.Surname);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // DeleteAutors
        public void DeleteAutor(int id)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("DeleteAutors", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        //  DeleteBooks
        public void DeleteBook(int id)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("DeleteBooks", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<int> GetAutorIdsByBook(int bookId)
        {
            var list = new List<int>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("SelectAutorsByBook", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BookId", bookId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                list.Add(Convert.ToInt32(reader["AutorId"]));

            return list;
        }

        public void AddAutorToBook(int bookId, int autorId)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("AddAutorToBook", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@BookId", bookId);
            cmd.Parameters.AddWithValue("@AutorId", autorId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void RemoveAutorFromBook(int bookId, int autorId)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("RemoveAutorFromBook", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@BookId", bookId);
            cmd.Parameters.AddWithValue("@AutorId", autorId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
