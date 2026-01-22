using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCLibrary.Data;
using MVCLibrary.Models;
using System.Text;


namespace MVCLibrary.Controllers
{
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _repo;

        public BooksController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAllBooks());
        }

        public IActionResult Create()
        {
            var autors = _repo.GetAllAutors();

            var vm = new BookCreateVm
            {
                AllAutors = autors.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.FullName
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateVm vm, IFormFile? contentsFile)
        {
            if (contentsFile != null && contentsFile.Length > 0)
            {
                using var reader = new StreamReader(contentsFile.OpenReadStream(), Encoding.UTF8);
                vm.Contents = await reader.ReadToEndAsync();
            }

            if (vm.SelectedAutorIds == null || vm.SelectedAutorIds.Count == 0)
                ModelState.AddModelError("", "Выберите хотя бы одного автора.");

            if (!ModelState.IsValid)
            {
                var autors = _repo.GetAllAutors();
                vm.AllAutors = autors.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.FullName
                }).ToList();
                return View(vm);
            }

            try
            {
                int newBookId = _repo.InsertBook(vm);

                foreach (var autorId in vm.SelectedAutorIds)
                    _repo.AddAutorToBook(newBookId, autorId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var autors = _repo.GetAllAutors();
                vm.AllAutors = autors.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.FullName
                }).ToList();

                return View(vm);
            }
        }

        public IActionResult Edit(int id)
        {
            var book = _repo.GetAllBooks().FirstOrDefault(x => x.ID == id);
            if (book == null) return NotFound();

            var allAutors = _repo.GetAllAutors();
            var selected = _repo.GetAutorIdsByBook(id);

            var vm = new BookEditVm
            {
                ID = book.ID,
                Title = book.Title,
                Year = book.Year,
                Edition = book.Edition,
                NumberPages = book.NumberPages,
                Contents = book.Contents,

                SelectedAutorIds = selected,
                AllAutors = allAutors.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.FullName
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookEditVm vm)
        {
            if (vm.SelectedAutorIds == null || vm.SelectedAutorIds.Count == 0)
                ModelState.AddModelError("", "Выберите хотя бы одного автора.");

            if (!ModelState.IsValid)
            {
                var all = _repo.GetAllAutors();
                vm.AllAutors = all.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.FullName
                }).ToList();
                return View(vm);
            }

            try
            {
            
                _repo.UpdateBook(vm.ID, vm.Title, vm.Year, vm.Edition, vm.NumberPages, vm.Contents);

                var current = _repo.GetAutorIdsByBook(vm.ID);

                var toAdd = vm.SelectedAutorIds.Except(current).ToList();
                var toRemove = current.Except(vm.SelectedAutorIds).ToList();

                foreach (var a in toAdd)
                    _repo.AddAutorToBook(vm.ID, a);

                foreach (var a in toRemove)
                    _repo.RemoveAutorFromBook(vm.ID, a);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var all = _repo.GetAllAutors();
                vm.AllAutors = all.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.FullName
                }).ToList();

                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.DeleteBook(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
