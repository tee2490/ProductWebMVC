using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Models;
using ProductWeb.ViewModels;
using ProductWeb.Services;
using ProductWeb.Utility;
using System.Security.Claims;

namespace ProductWeb.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ProductContext productContext;
        private readonly ShoppingCartService shoppingCartService;
        private readonly IWebHostEnvironment webHostEnvironment;

        [BindProperty] //ให้ผูกออบเจคโดยอัตโนมัติเทียบเท่าการส่งพารามิเตอร์
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(ProductContext productContext, ShoppingCartService shoppingCartService,
            IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.shoppingCartService = shoppingCartService;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = productContext.ShoppingCarts.Include(x => x.Product).Where(u => u.UserId.Equals(claim.Value)),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Product.Price * cart.Count;
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = productContext.ShoppingCarts.Find(cartId);
            shoppingCartService.IncrementCount(cart, 1);
            shoppingCartService.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = productContext.ShoppingCarts.Find(cartId);

            if (cart.Count > 1)
            {
                shoppingCartService.DecrementCount(cart, 1);
                shoppingCartService.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = productContext.ShoppingCarts.Find(cartId);
            productContext.Remove(cart);
            productContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = productContext.ShoppingCarts.Include(x => x.Product).Where(u => u.UserId.Equals(claim.Value)),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.User = productContext.Users.Find(claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.User.FullName;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.User.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.User.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.User.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.User.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(IFormFile file)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            ShoppingCartVM.ListCart = productContext.ShoppingCarts.Include(x => x.Product).Where(u => u.UserId.Equals(claim.Value));

            ShoppingCartVM.OrderHeader.UserId = claim.Value;
            ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Product.Price * cart.Count);
            }

            var user = productContext.Users.Find(claim.Value);

            #region Image Management
            string wwwRootPath = webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(file.FileName);
                var uploads = Path.Combine(wwwRootPath, @"images\payments");

                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                //บันทึกรุปภาพใหม่
                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                ShoppingCartVM.OrderHeader.PaymentImage = @"\images\payments\" + fileName + extension;
            }
            #endregion

            productContext.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
            productContext.SaveChanges();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Count = cart.Count
                };
                productContext.OrderDetails.Add(orderDetail);
                // productContext.SaveChanges();
            }

            productContext.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCart);
            productContext.SaveChanges();

            TempData["success"] = "successful transaction";
            return RedirectToAction("Index", "Home");
        }

    }
}
