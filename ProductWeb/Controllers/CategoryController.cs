using Microsoft.AspNetCore.Mvc;
using ProductWeb.Data;
using ProductWeb.Models;

namespace ProductWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ProductContext productContext;

        public CategoryController(ProductContext productContext)
        {
            this.productContext = productContext;
        }
        public IActionResult Index()
        {
            var data=productContext.Categories.ToList();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category data)
        {
            if (ModelState.IsValid)
            {
                productContext.Categories.Add(data);
                productContext.SaveChanges();
                TempData["success"] = "Create success fully";
                return RedirectToAction("Index");
            }
            return View(data);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var data = productContext.Categories.Find(id);
            if (data == null) return NotFound();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category data)
        {
            if (ModelState.IsValid)
            {
                productContext.Categories.Update(data);
                productContext.SaveChanges();

                TempData["success"] = "Edit success fully";
                return RedirectToAction("Index");
            }
            return View(data);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var data = productContext.Categories.Find(id);
            if (data == null) return NotFound();
            productContext.Categories.Remove(data);
            productContext.SaveChanges();

            TempData["success"] = "Delete success fully";
            return RedirectToAction("Index");
        }


    }
}
