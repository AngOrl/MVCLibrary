using MVCLibrary.Data;
using MVCLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace MVCLibrary.Controllers
{
    public class AutorsController : Controller
    {
        private readonly ILibraryRepository _repo;

        public AutorsController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAllAutors());
        }

        public IActionResult Create()
        {
            return View(new AutorCreateVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AutorCreateVm vm)
        {
            try
            {
                _repo.InsertAutor(vm.FirstName, vm.Surname);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }


        public IActionResult Edit(int id)
        {
            var autor = _repo.GetAllAutors().FirstOrDefault(a => a.ID == id);
            if (autor == null) return NotFound();

            var vm = new AutorEditVm
            {
                ID = autor.ID,
                FirstName = autor.FirstName,
                Surname = autor.Surname
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AutorEditVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                _repo.UpdateAutor(vm);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _repo.DeleteAutor(id);
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
