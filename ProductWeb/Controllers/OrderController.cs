using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.ViewModels;
using ProductWeb.Utility;

namespace ProductWeb.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ProductContext productContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(ProductContext productContext, IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var data = productContext.OrderHeaders;
            return View(data);
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = productContext.OrderHeaders.Include(x => x.User).FirstOrDefault(x => x.Id.Equals(orderId)),
                OrderDetail = productContext.OrderDetails.Include(x => x.Product).Where(x => x.OrderId.Equals(orderId)).ToList()
            };
            return View(OrderVM);
        }

        public IActionResult Delete(int orderId)
        {
            var orderHeaderFromDb = productContext.OrderHeaders.Find(orderId);

            #region Image Management ลบรูปภาพบิลชำระเงิน
            string wwwRootPath = webHostEnvironment.WebRootPath;

            if (orderHeaderFromDb.PaymentImage != null)
            {
                var oldImagePath = Path.Combine(wwwRootPath, orderHeaderFromDb.PaymentImage.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            #endregion

            productContext.Remove(orderHeaderFromDb);
            productContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderHeader()
        {
            var orderHeaderFromDb = productContext.OrderHeaders.Find(OrderVM.OrderHeader.Id);

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            productContext.OrderHeaders.Update(orderHeaderFromDb);
            productContext.SaveChanges();
            TempData["Success"] = "Order Header Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }


        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StatusOrder(string status)
        {
            var orderHeaderFromDb = productContext.OrderHeaders.Find(OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb.OrderStatus == SD.StatusPending)
            {
                orderHeaderFromDb.OrderStatus = status;
                TempData["Success"] = "Status has been updated Succesfully.";
                productContext.SaveChanges();
            }
            else
            {
                TempData["Success"] = "Can't update because status has ended.";
            }

            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

    }

}
