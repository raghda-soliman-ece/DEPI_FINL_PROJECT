using Jumia.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Jumia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private static readonly (string[] Keys, string Reply)[] Faqs =
        {
            (new[] { "مرحبا", "اهلا", "السلام", "hello", "hi", "هاي" },
                "أهلاً بيك في جوميا 👋 قولي عايز مساعدة في إيه؟ (شحن، طلبات، حساب، سلة، منتجات...)"),

            (new[] { "شحن", "توصيل", "متى يوصل", "التوصيل", "shipping", "delivery" },
                "التوصيل عادة من 2 لـ 5 أيام عمل حسب المحافظة. الطلبات فوق حد معين ممكن يبقى الشحن مجاني."),

            (new[] { "طلب", "اوردر", "تتبع", "فين طلبي", "order", "track" },
                "تتبع الطلبات من صفحة حسابي. لو مسجل دخول هتشوف كل طلباتك هناك."),

            (new[] { "سلة", "كارت", "cart", "اضيف", "اشتري" },
                "تقدر تضيف منتجات بزر «أضف للسلة»، وبعدين من صفحة السلة تكمل الشراء. زر «اشتري الآن» بيحوّل المنتج للسلة على طول."),

            (new[] { "حساب", "تسجيل", "لوجين", "login", "register", "باسورد", "كلمة المرور" },
                "سجّل من صفحة تسجيل الدخول. بعد التسجيل/اللوجين هتقدر تتابع الطلبات والمفضلة."),

            (new[] { "دفع", "كاش", "فيزا", "فودافون", "instapay", "payment" },
                "طرق الدفع المتاحة عندنا: الدفع عند الاستلام، بطاقة بنكية، فودافون كاش، و InstaPay."),

            (new[] { "ارجاع", "استرجاع", "رجع", "return", "استبدال" },
                "تقدر تطلب استرجاع خلال 14 يوم لو المنتج بنفس حالته. تواصل مع الدعم من حسابك."),

            (new[] { "كوبون", "خصم", "coupon", "عرض" },
                "جرب كوبونات زي: JUMIA10 أو WELCOME50 أو SALE20 في صفحة إتمام الشراء."),

            (new[] { "منتج", "موبايل", "تلفزيون", "بحث", "product", "search" },
                "استخدم شريط البحث فوق أو تصفح الأقسام: موبايلات، إلكترونيات، أزياء، سوبر ماركت، وألعاب أطفال."),

            (new[] { "مفضلة", "wishlist", "قلب" },
                "اضغط على أيقونة القلب على المنتج عشان تضيفه للمفضلة، وهتلاقيها في حسابك."),

            (new[] { "شكر", "thanks", "تمام", "اوك", "ok" },
                "العفو 🙌 لو محتاج حاجة تانية قولي."),
        };

        private static readonly string[] Suggestions =
        {
            "فين الشحن؟",
            "ازاي اتابع طلبي؟",
            "طرق الدفع؟",
            "عايز كوبون خصم"
        };

        private const string Fallback =
            "مش فاهم قصدك كويس 😅 جرب تسأل عن: الشحن، الطلبات، السلة، الحساب، الدفع، أو الكوبونات.";

        [HttpPost]
        public ActionResult<ChatReplyDto> Ask([FromBody] ChatRequestDto request)
        {
            var text = (request.Message ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(text))
            {
                return Ok(new ChatReplyDto
                {
                    Reply = "اكتب سؤالك هنا وأنا هساعدك.",
                    Suggestions = Suggestions
                });
            }

            foreach (var (keys, reply) in Faqs)
            {
                if (keys.Any(k => text.Contains(k.ToLowerInvariant())))
                {
                    return Ok(new ChatReplyDto { Reply = reply, Suggestions = Suggestions });
                }
            }

            return Ok(new ChatReplyDto { Reply = Fallback, Suggestions = Suggestions });
        }

        [HttpGet("suggestions")]
        public ActionResult<object> GetSuggestions()
        {
            return Ok(new { suggestions = Suggestions });
        }
    }
}
