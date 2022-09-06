using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Models;
using ProductWeb.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace ProductWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductContext productContext;
        private readonly ShoppingCartService shoppingCartService;

        public HomeController(ProductContext productContext, ShoppingCartService shoppingCartService)
        {
            this.productContext = productContext;
            this.shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            var data = productContext.Products.Include(x => x.Category);
            return View(data);
        }


        public IActionResult Details(int productId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = productContext.Products.Include(x => x.Category)
                                        .FirstOrDefault(x => x.Id.Equals(productId)),
            };

            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.UserId = claim.Value; //ใครทำรายการ

            ShoppingCart cartFromDb = productContext.ShoppingCarts.FirstOrDefault(
                u => u.UserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb == null)
            {
                shoppingCartService.Add(shoppingCart);
                shoppingCartService.Save();
            }
            else
            {
                shoppingCartService.IncrementCount(cartFromDb, shoppingCart.Count);
                shoppingCartService.Save();
            }


            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}