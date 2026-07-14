/* Simple Jumia FAQ chatbot — works on every page */
(function () {
    const FAQ_FALLBACK = [
        { keys: ['مرحبا', 'اهلا', 'السلام', 'hello', 'hi', 'هاي'], reply: 'أهلاً بيك في جوميا 👋 قولي عايز مساعدة في إيه؟' },
        { keys: ['شحن', 'توصيل', 'يوصل', 'shipping', 'delivery'], reply: 'التوصيل عادة من 2 لـ 5 أيام عمل حسب المحافظة.' },
        { keys: ['طلب', 'اوردر', 'تتبع', 'order', 'track'], reply: 'تتبع الطلبات من صفحة حسابي بعد تسجيل الدخول.' },
        { keys: ['سلة', 'cart', 'اشتري'], reply: 'ضيف المنتج للسلة وبعدين كمل من صفحة السلة أو اضغط اشتري الآن.' },
        { keys: ['حساب', 'تسجيل', 'لوجين', 'login', 'register'], reply: 'سجّل من صفحة تسجيل الدخول عشان تتابع طلباتك ومفضلتك.' },
        { keys: ['دفع', 'كاش', 'فيزا', 'فودافون', 'instapay', 'payment'], reply: 'الدفع عند الاستلام، بطاقة، فودافون كاش، و InstaPay.' },
        { keys: ['كوبون', 'خصم', 'coupon'], reply: 'جرب: JUMIA10 أو WELCOME50 أو SALE20 عند الدفع.' },
        { keys: ['ارجاع', 'استرجاع', 'return'], reply: 'الاسترجاع خلال 14 يوم لو المنتج بحالته.' },
        { keys: ['شكر', 'thanks', 'تمام'], reply: 'العفو 🙌' }
    ];

    const SUGGESTIONS = ['فين الشحن؟', 'ازاي اتابع طلبي؟', 'طرق الدفع؟', 'عايز كوبون خصم'];

    function localReply(msg) {
        const t = (msg || '').toLowerCase();
        for (const row of FAQ_FALLBACK) {
            if (row.keys.some(k => t.includes(k.toLowerCase()))) return row.reply;
        }
        return 'مش فاهم قصدك كويس 😅 جرب تسأل عن الشحن، الطلبات، السلة، الحساب، أو الدفع.';
    }

    function injectStyles() {
        if (document.getElementById('jumia-chat-styles')) return;
        const style = document.createElement('style');
        style.id = 'jumia-chat-styles';
        style.textContent = `
#jumia-chat-btn{
  position:fixed !important;
  bottom:22px !important;
  right:22px !important;
  left:auto !important;
  z-index:2147483000 !important;
  width:58px;height:58px;border:none;border-radius:50%;
  background:#f68b1e !important;color:#fff !important;cursor:pointer;
  box-shadow:0 8px 24px rgba(0,0,0,.28);font-size:26px;line-height:1;
  display:flex !important;align-items:center;justify-content:center;
  visibility:visible !important;opacity:1 !important;
}
#jumia-chat-panel{
  position:fixed !important;bottom:92px !important;right:22px !important;left:auto !important;
  z-index:2147483000 !important;
  width:min(340px,calc(100vw - 32px));height:440px;
  background:#fff;border-radius:16px;overflow:hidden;
  box-shadow:0 16px 40px rgba(0,0,0,.25);
  display:none;flex-direction:column;font-family:Tajawal,sans-serif;direction:rtl;
}
#jumia-chat-panel.open{display:flex !important}
.jumia-chat-head{
  background:#f68b1e;color:#fff;padding:14px 16px;font-weight:800;
  display:flex;align-items:center;justify-content:space-between;gap:8px;
}
.jumia-chat-head button{background:transparent;border:none;color:#fff;font-size:18px;cursor:pointer}
.jumia-chat-msgs{flex:1;overflow:auto;padding:14px;background:#f7f7f7;display:flex;flex-direction:column;gap:10px}
.jumia-chat-bubble{max-width:85%;padding:10px 12px;border-radius:12px;font-size:13px;line-height:1.55;white-space:pre-wrap}
.jumia-chat-bubble.bot{align-self:flex-start;background:#fff;border:1px solid #eee;color:#333}
.jumia-chat-bubble.user{align-self:flex-end;background:#fff3e6;color:#333}
.jumia-chat-suggestions{display:flex;flex-wrap:wrap;gap:6px;padding:8px 12px;background:#fff;border-top:1px solid #eee}
.jumia-chat-suggestions button{
  border:1px solid #f0d2ad;background:#fff8f0;color:#c45e00;
  border-radius:999px;padding:6px 10px;font-size:11px;cursor:pointer;font-family:inherit
}
.jumia-chat-form{display:flex;gap:8px;padding:10px;background:#fff;border-top:1px solid #eee}
.jumia-chat-form input{
  flex:1;border:1.5px solid #e5e5e5;border-radius:10px;padding:10px 12px;
  font-family:inherit;font-size:13px;outline:none
}
.jumia-chat-form button{
  border:none;background:#f68b1e;color:#fff;border-radius:10px;
  padding:0 14px;cursor:pointer;font-weight:700;font-family:inherit
}`;
        document.head.appendChild(style);
    }

    function addBubble(box, text, who) {
        const div = document.createElement('div');
        div.className = 'jumia-chat-bubble ' + who;
        div.textContent = text;
        box.appendChild(div);
        box.scrollTop = box.scrollHeight;
    }

    async function askApi(message) {
        const base = (typeof API_BASE_URL !== 'undefined') ? API_BASE_URL : '';
        if (!base) return localReply(message);
        try {
            const res = await fetch(`${base}/Chat`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message })
            });
            if (!res.ok) throw new Error('chat api failed');
            const data = await res.json();
            return data.reply || data.Reply || localReply(message);
        } catch (e) {
            return localReply(message);
        }
    }

    function build() {
        if (!document.body) return;
        if (document.getElementById('jumia-chat-btn')) return;

        injectStyles();

        const btn = document.createElement('button');
        btn.id = 'jumia-chat-btn';
        btn.type = 'button';
        btn.title = 'مساعد جوميا';
        btn.setAttribute('aria-label', 'مساعد جوميا');
        btn.textContent = '💬';

        const panel = document.createElement('div');
        panel.id = 'jumia-chat-panel';
        panel.innerHTML = `
          <div class="jumia-chat-head">
            <span>🤖 مساعد جوميا</span>
            <button type="button" id="jumia-chat-close" aria-label="close">×</button>
          </div>
          <div class="jumia-chat-msgs" id="jumia-chat-msgs"></div>
          <div class="jumia-chat-suggestions" id="jumia-chat-suggestions"></div>
          <form class="jumia-chat-form" id="jumia-chat-form">
            <input type="text" id="jumia-chat-input" placeholder="اكتب سؤالك..." autocomplete="off" />
            <button type="submit">إرسال</button>
          </form>`;

        document.body.appendChild(btn);
        document.body.appendChild(panel);

        const msgs = panel.querySelector('#jumia-chat-msgs');
        const form = panel.querySelector('#jumia-chat-form');
        const input = panel.querySelector('#jumia-chat-input');
        const sugBox = panel.querySelector('#jumia-chat-suggestions');

        addBubble(msgs, 'أهلاً! أنا مساعد جوميا البسيط. اسأل عن الشحن، الطلبات، الدفع، أو الكوبونات.', 'bot');

        SUGGESTIONS.forEach(text => {
            const b = document.createElement('button');
            b.type = 'button';
            b.textContent = text;
            b.addEventListener('click', () => {
                input.value = text;
                form.requestSubmit();
            });
            sugBox.appendChild(b);
        });

        btn.addEventListener('click', () => {
            panel.classList.toggle('open');
            if (panel.classList.contains('open')) input.focus();
        });
        panel.querySelector('#jumia-chat-close').addEventListener('click', () => panel.classList.remove('open'));

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const message = input.value.trim();
            if (!message) return;
            addBubble(msgs, message, 'user');
            input.value = '';
            const thinking = document.createElement('div');
            thinking.className = 'jumia-chat-bubble bot';
            thinking.textContent = '...';
            msgs.appendChild(thinking);
            msgs.scrollTop = msgs.scrollHeight;
            const reply = await askApi(message);
            thinking.remove();
            addBubble(msgs, reply, 'bot');
        });
    }

    window.initJumiaChat = build;

    function boot() {
        try { build(); } catch (e) { console.warn('chat widget', e); }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
    window.addEventListener('load', boot);
})();
