using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Models;
using ProductWeb.ViewModels;

namespace ProductWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductContext productContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(ProductContext productContext,IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            TempData["count"] = productContext.Products.Count();
            var data = productContext.Products.Include(x=>x.Category);
            return View(data);
        }


        public IActionResult UpSert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new() { Name = "TestProduct", Price = 1, Description = "Test Descript" },
                CategoryList = productContext.Categories.Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };


            if (id == null || id == 0)
            {
            }
            else
            {
                productVM.Product = productContext.Products.Find(id);
            }
            return View(productVM);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(ProductVM data, IFormFile file)
        {
            ModelState.Remove("file"); //ยกเลิกการตรวจสอบบางฟิลด์

            if (ModelState.IsValid)
            {
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(file.FileName);
                    var uploads = Path.Combine(wwwRootPath, @"images\products");

                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);


                    //กรณีมีรูปภาพเดิมต้องลบทิ้งก่อน
                    if (data.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, data.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    //บันทึกรุปภาพใหม่
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    data.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (data.Product.Id == 0)
                {
                    productContext.Products.Add(data.Product);
                }
                else
                {
                    productContext.Products.Update(data.Product);
                }

                productContext.SaveChanges();
                TempData["success"] = "Product managed successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(data);
        }


        public IActionResult Delete(int? id)
        {
            var data= productContext.Products.Find(id);
            if (data == null)
            {
                return NotFound();
            }

            var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, data.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            productContext.Products.Remove(data);
            productContext.SaveChanges();
            TempData["success"] = "Delete Successful";
            return RedirectToAction(nameof(Index));

        }

    }
}
