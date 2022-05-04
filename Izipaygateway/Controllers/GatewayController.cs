using Izipaygateway.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Izipaygateway.Controllers
{
    public class GatewayController : Controller
    {
        [HttpPost]
        public IActionResult Index()
        {
            var nombre = HttpContext.Request.Form["txt_name"];
            var apellido = HttpContext.Request.Form["txt_lastname"];
            var correo = HttpContext.Request.Form["txt_email"];
            Task<string> task1 = api_post(nombre,apellido,correo);
            string valorjson = task1.Result;
            var jsondeserialze = JsonConvert.DeserializeObject<dynamic>(valorjson);
            ViewBag.FORMTOKEN = jsondeserialze.answer.formToken;
            return View();
        }

        [HttpPost]
        public IActionResult respuesta()
        {
            var valor = HttpContext.Request.Form["kr-answer"];
            var jsondeserialze = JsonConvert.DeserializeObject<dynamic>(valor);
            ViewBag.ORDERSTATUS = jsondeserialze.orderStatus;
            ViewBag.ORDERTOTLAAMOUNT = jsondeserialze.orderDetails.orderTotalAmount;
            ViewBag.ORDERID = jsondeserialze.orderDetails.orderId;
            ViewBag.EMAIL = jsondeserialze.customer.email;
            return View();
        }

        [HttpPost]
        [ActionName("validador")]
        public string validador()
        {
            GatewayModel gm = new GatewayModel();
            var validador = HttpContext.Request;
            string key = "";
            var calculatedHash = "";
            if ("sha256_hmac" != validador.Form["kr_hash_algorithm"])
            {
                return JsonConvert.SerializeObject("false");
            }
            string krAnswer = validador.Form["kr_answer"].ToString().Replace("\\/", "/");
            if (validador.Form["kr_hash_key"] == "sha256_hmac")
            {
                key = gm.KEY_SHA256;
            }
            else if (validador.Form["kr_hash_key"] == "password")
            {
                key = gm.KEY_PASSWORD;
            }
            else
            {
                return JsonConvert.SerializeObject("false");
            }

            calculatedHash = hmac256(key,krAnswer);
            if (calculatedHash == validador.Form["kr_hash"]) return JsonConvert.SerializeObject("true");
            calculatedHash = hmac256(key, Decoder(krAnswer));
            if (calculatedHash == validador.Form["kr_hash"]) return JsonConvert.SerializeObject("true");
            return JsonConvert.SerializeObject("false");
        }

        public string Decoder(string value)
        {
            Encoding enc = new UTF8Encoding();
            byte[] bytes = enc.GetBytes(value);
            return enc.GetString(bytes);
        }

        public string hmac256(string key, string krAnswer)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            Byte[] code = encoder.GetBytes(key);
            HMACSHA256 hmSha256 = new HMACSHA256(code);
            Byte[] hashMe = encoder.GetBytes(krAnswer);
            Byte[] hmBytes = hmSha256.ComputeHash(hashMe);
            return ToHexString(hmBytes);
        }

        public static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public String encriptado()
        {
            GatewayModel gm = new GatewayModel();
            string str = gm.KEY_USER + ":" + gm.KEY_PASSWORD;
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        public async Task<String> api_post(String nombre, String apellido, String correo)
        {
            GatewayModel gm = new GatewayModel();
            string path = "api-payment/V4/Charge/CreatePayment";
            var objjson = new
            {
                amount = 180,
                currency = "PEN",
                orderId = "MyOrderId-123456",
                customer = new
                {
                    email = correo,
                    billingDetails = new
                    {
                        firstName = nombre,
                        lastName = apellido,
                        phoneNumber = "987987654",
                        address = "AV LIMA 123",
                        address2 = "AV LIMA 1234"
                    }
                }
            };
            var json = JsonConvert.SerializeObject(objjson);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + encriptado());
            var response = await client.PostAsync(gm.URL_BASE+path, data);
            string result = response.Content.ReadAsStringAsync().Result;
            client.Dispose();
            return result;
        }
    }
}
