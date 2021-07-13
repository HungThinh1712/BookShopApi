using BookShopApi.Functions;
using BookShopApi.Models;
using BookShopApi.Models.ViewModels;
using BookShopApi.Momo;
using BookShopApi.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartsController : ControllerBase
    {
        private readonly ShoppingCartService _shoppingCartService;
        private readonly PromotionService _promotionService;
        private readonly BookService _bookService;
        private readonly OrderService _orderService;
        private readonly UserService _userService;

        public ShoppingCartsController(ShoppingCartService shoppingCartService,
            BookService bookService,
            OrderService orderService, PromotionService promotionService,UserService userService)
        {
            _shoppingCartService = shoppingCartService;
            _bookService = bookService;
            _orderService = orderService;
            _promotionService = promotionService;
            _userService = userService;
        }

       

        [HttpGet("[action]")]
        public async Task<ActionResult<ShoppingCart>> Get()
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var shoppingCart = await _shoppingCartService.GetByUserIdAsync(userId);
            if (shoppingCart == null)
            {
                return BadRequest("shoppingCart not found");
            }
            return Ok(shoppingCart.ItemInCart.Adapt<List<ItemInCartViewModel>>());
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> AddToCart(
            [FromBody] JObject shoppingCart)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var shoppingCartInDB = await _shoppingCartService.GetByUserIdAsync(userId);
            if (shoppingCartInDB.ItemInCart == null)
                shoppingCartInDB.ItemInCart = new List<ItemInCart>();
            bool isExisted = false;
            if (shoppingCart["shoppingCartData"] != null)
            {
                var shoppingCartInBody = shoppingCart["shoppingCartData"];

              
                List<ItemInCart> itemInCarts = shoppingCartInBody
                    .Select(sc =>
                            new ItemInCart()
                            {
                                 BookId   =  sc["bookId"].ToString(),  
                                 Amount   = int.Parse(sc["amount"].ToString()),    
                                 Name      = sc["name"].ToString(),
                                Price      = Convert.ToDecimal(sc["price"].ToString()),
                                CoverPrice = Convert.ToDecimal(sc["coverPrice"].ToString()),
                                AuthorName = sc["authorName"].ToString(),
                                ImageSrc      = sc["imageSrc"].ToString(),
                            }
                            ).ToList();
                foreach (var itemInBody in itemInCarts)
                {
                    foreach (var itemInDB in shoppingCartInDB.ItemInCart)
                    {
                        if (itemInDB.BookId == itemInBody.BookId)
                        {
                            itemInDB.Amount = itemInDB.Amount + itemInBody.Amount;
                            isExisted = true;
                        }

                    }
                    if (!isExisted)
                    {
                        shoppingCartInDB.ItemInCart.Add(itemInBody);
                    }
                    isExisted = false;
                }
            }
            await _shoppingCartService.UpdateAsync(userId, shoppingCartInDB);
            return Ok(shoppingCartInDB);


        }

        [HttpDelete("[action]")]
        public async Task<ActionResult> DeleteItemInCart(string bookId)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            await _shoppingCartService.RemoveCartItemAsync(userId, bookId);
            return Ok("Delete successfull");
        }
        [HttpGet("[action]")]
        public async Task<ActionResult> PayForCart(int paymentType,decimal shippingFee, decimal totalMoney,string promotionCode)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var shoppingcart = await _shoppingCartService.GetByUserIdAsync(userId);
            Promotion promotion = null;
            if(promotionCode != null)
            {
                promotion = await _promotionService.GetAsync(promotionCode);
                await _promotionService.UpdatePromotionAfterApply(userId, promotionCode);
            }

            foreach (var item in shoppingcart.ItemInCart)
            {
                var book = await _bookService.GetAsync(item.BookId);
                item.TotalMoney = item.Amount * item.Price;
                book.Amount = book.Amount - item.Amount;
                await _bookService.UpdateAsync(book.Id, book);
            }
            var user = await _userService.GetAsync(userId);
            var order = await _orderService.CreateAsync(user, paymentType, shoppingcart.ItemInCart,shippingFee,totalMoney, promotion);
            shoppingcart.ItemInCart.Clear();
            await _shoppingCartService.UpdateAsync(userId, shoppingcart);

            return Ok(order);
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateAmountItemInCart([FromBody] JObject body)
        {
            //variables
            string userId = body["userId"].ToString();
            string bookId = body["bookId"].ToString();
            int amount = Int32.Parse(body["amount"].ToString());


            var shoppingcart = await _shoppingCartService.GetByUserIdAsync(userId);

            foreach (var item in shoppingcart.ItemInCart)
            {
                if (item.BookId == bookId)
                {
                    item.Amount = amount;
                    break;
                }
            }
            await _shoppingCartService.UpdateAsync(userId, shoppingcart);

            return Ok("Cập nhật thành công");
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateAmountCartItemEqualsToDB()
        {
            string message = "";
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            var shoppingCartInDB = await _shoppingCartService.GetByUserIdAsync(userId);

            foreach (var itemInDB in shoppingCartInDB.ItemInCart)
            {
                var book = await _bookService.GetAsync(itemInDB.BookId);
                if (book.Amount < itemInDB.Amount)
                {
                    message = message + string.Format("Sản phẩm {0} hiện tại chỉ còn {1} sản phẩm. Vui lòng cập nhật lại số lượng", itemInDB.Name, book.Amount.ToString());
                }

            }
            if (message != "")
            {
                return BadRequest(message);
            }
            else
            {
                return Ok("Pass");
            }

        }

        [HttpPut("[action]")]
        public async Task<IActionResult> IncreaseOrDecreaseItemAmount(
            string bookId,int amount)
        {
            var headerValues = Request.Headers["Authorization"];
            string userId = Authenticate.DecryptToken(headerValues.ToString());
            
            return Ok((await _shoppingCartService.UpdateAmountAsync(userId, bookId,amount)).Adapt<List<ItemInCartViewModel>>());
           

        }

        [HttpGet("[action]")]
        public  ActionResult PayByMomo(string totalMoney, string shippingFee, string promotionCode)
        {         
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOBKUN20180529";
            string accessKey = "klm05TvNBzhg7h7j";
            string serectkey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
            string orderInfo = "Thanh toán đơn hàng";
            string returnUrl = "https://bookshoptina.herokuapp.com/order_success_page";
            returnUrl = returnUrl + string.Format("?shippingFee={0}", shippingFee);
            if (promotionCode != null)
            {
                returnUrl = returnUrl + string.Format("&promotionCode={0}", promotionCode);
            }
            string notifyurl = "https://momo.vn/notify";
            string amount = totalMoney;
            string orderid = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "merchantName=Payment";


            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
               partnerCode + "&accessKey=" +
               accessKey + "&requestId=" +
               requestId + "&amount=" +
               amount + "&orderId=" +
               orderid + "&orderInfo=" +
               orderInfo + "&returnUrl=" +
               returnUrl + "&notifyUrl=" +
               notifyurl + "&extraData=" +
               extraData;



            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);


            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };
            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());
            JObject jMessage = JObject.Parse(responseFromMomo);
            return Ok(jMessage["payUrl"].ToString());
        }

        private ItemInCart GetItemInCartExisted(List<ItemInCart> cartInBody,List<ItemInCart> cartInDB)
        {
            foreach(var itemInBody in cartInBody)
            {
                foreach(var itemInDB in cartInDB)
                {
                    if (itemInDB == itemInBody)
                        return itemInDB;
                }
            }
            return null;
        }
    }
}