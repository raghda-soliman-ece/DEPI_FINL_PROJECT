/* ============================================
   JUMIA EGYPT - Main Script
   Features: Cart, Wishlist, User Session, Coupons, Orders
============================================ */

// ========== USER SESSION ==========
let currentUser = JSON.parse(localStorage.getItem('jumia-user') || 'null');

function updateHeaderUser() {
    const loginLinks = document.querySelectorAll('.login-link');
    const userMenus = document.querySelectorAll('.user-menu');

    if (currentUser) {
        loginLinks.forEach(el => {
            el.innerHTML = `<i class="far fa-user-circle"></i> <span>${currentUser.firstName}</span>`;
            el.href = 'account.html';
            el.style.color = 'var(--primary)';
        });
    }
}

function logout() {
    localStorage.removeItem('jumia-user');
    currentUser = null;
    window.location.href = 'index.html';
}

// ========== CART ==========
let cartItems = JSON.parse(localStorage.getItem('jumia-cart') || '[]');

function updateCartUI() {
    const count = cartItems.reduce((sum, i) => sum + i.qty, 0);
    document.querySelectorAll('.cart-count').forEach(el => el.textContent = count);
}

function addToCart(btn, name, price) {
    const existing = cartItems.find(i => i.name === name);
    const imgEl = btn.closest('.product-card')?.querySelector('img');
    if (existing) {
        existing.qty++;
    } else {
        cartItems.push({ name, price, qty: 1, img: imgEl?.src || '' });
    }
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    updateCartUI();
    const orig = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-check"></i> تم الإضافة!';
    btn.style.background = '#4caf50';
    setTimeout(() => { btn.innerHTML = orig; btn.style.background = ''; }, 1500);
    showToast(`تمت إضافة "${name}" للسلة 🛒`);
}

// ========== WISHLIST ==========
let wishlist = JSON.parse(localStorage.getItem('jumia-wishlist') || '[]');

function toggleWishlist(btn, name, price) {
    const imgEl = btn.closest('.product-card')?.querySelector('img');
    const idx = wishlist.findIndex(i => i.name === name);
    if (idx === -1) {
        wishlist.push({ name, price, img: imgEl?.src || '' });
        btn.classList.add('wishlisted');
        btn.innerHTML = '<i class="fas fa-heart"></i>';
        showToast(`تمت الإضافة للمفضلة ❤️`);
    } else {
        wishlist.splice(idx, 1);
        btn.classList.remove('wishlisted');
        btn.innerHTML = '<i class="far fa-heart"></i>';
        showToast(`تمت الإزالة من المفضلة`);
    }
    localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
    updateWishlistCount();
}

function updateWishlistCount() {
    document.querySelectorAll('.wishlist-count').forEach(el => el.textContent = wishlist.length);
}

function isWishlisted(name) {
    return wishlist.some(i => i.name === name);
}

function highlightWishlistedItems() {
    const wishNames = wishlist.map(w => w.name);
    document.querySelectorAll('.wishlist-btn').forEach(btn => {
        const onclickAttr = btn.getAttribute('onclick');
        if (onclickAttr) {
            const match = onclickAttr.match(/toggleWishlist\([^,]+,\s*(['"])(.*?)\1/);
            if (match && wishNames.includes(match[2])) {
                btn.classList.add('wishlisted');
                btn.innerHTML = '<i class="fas fa-heart"></i>';
            }
        }
    });
}

// ========== TOAST ==========
function showToast(msg) {
    let toast = document.getElementById('toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'toast';
        toast.className = 'toast';
        document.body.appendChild(toast);
    }
    toast.textContent = msg;
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 3000);
}

// ========== CART PAGE ==========
function renderCart() {
    const container = document.getElementById('cart-items');
    const totalEl = document.getElementById('cart-total');
    const subtotalEl = document.getElementById('cart-subtotal');
    const countEl = document.getElementById('cart-heading');
    if (!container) return;

    if (!cartItems.length) {
        container.innerHTML = `
            <div class="empty-cart">
                <i class="fas fa-shopping-cart"></i>
                <h3>سلتك فارغة</h3>
                <p>أضف منتجات من المتجر دلوقتي!</p>
                <a href="index.html" class="btn btn-orange" style="margin-top:20px;display:inline-flex;">تسوق الآن</a>
            </div>`;
        if (totalEl) totalEl.textContent = '0 EGP';
        if (subtotalEl) subtotalEl.textContent = '0 EGP';
        if (countEl) countEl.textContent = 'سلة التسوق (0 منتجات)';
        return;
    }

    const total = cartItems.reduce((s, i) => s + i.price * i.qty, 0);
    if (countEl) countEl.textContent = `سلة التسوق (${cartItems.length} منتجات)`;
    if (totalEl) totalEl.textContent = total.toLocaleString() + ' EGP';
    if (subtotalEl) subtotalEl.textContent = total.toLocaleString() + ' EGP';

    container.innerHTML = cartItems.map((item, idx) => `
        <div class="cart-item">
            <img src="${item.img || 'https://placehold.co/90x90'}" alt="${item.name}" onerror="this.src='https://placehold.co/90x90/f5f5f5/999?text=Item'">
            <div class="cart-item-info">
                <h4>${item.name}</h4>
                <div class="cart-item-actions">
                    <div class="qty-box">
                        <button onclick="changeQty(${idx}, -1)">−</button>
                        <span>${item.qty}</span>
                        <button onclick="changeQty(${idx}, 1)">+</button>
                    </div>
                    <span class="remove-btn" onclick="removeItem(${idx})"><i class="fas fa-trash"></i> حذف</span>
                    <span class="remove-btn" style="color:#2196f3;" onclick="moveToWishlist(${idx})"><i class="far fa-heart"></i> حفظ للمفضلة</span>
                </div>
            </div>
            <div class="cart-price">EGP ${(item.price * item.qty).toLocaleString()}</div>
        </div>`).join('');
}

function changeQty(idx, delta) {
    cartItems[idx].qty += delta;
    if (cartItems[idx].qty <= 0) cartItems.splice(idx, 1);
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    renderCart(); updateCartUI();
}

function removeItem(idx) {
    cartItems.splice(idx, 1);
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    renderCart(); updateCartUI();
}

function moveToWishlist(idx) {
    const item = cartItems[idx];
    if (!wishlist.find(w => w.name === item.name)) {
        wishlist.push({ name: item.name, price: item.price, img: item.img });
        localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
        updateWishlistCount();
    }
    cartItems.splice(idx, 1);
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    renderCart(); updateCartUI();
    showToast('تم نقل المنتج للمفضلة ❤️');
}

// ========== SEARCH ==========
const globalSearchDB = [
    { title: 'زيت زيتون Colavita خالص 750 مل', price: '189', img: 'images/oil.jpg' },
    { title: 'لبن Baraka كامل الدسم 1 لتر × 6', price: '155', img: 'images/baraka.jpg' },
    { title: 'شوكولاتة Ferrero Rocher 30 حبة', price: '299', img: 'images/ferrero.jpg' },
    { title: 'مسحوق غسيل Ariel 5 كيلو', price: '279', img: 'images/ariel.jpg' },
    { title: 'قهوة Nescafé Classic 200 جرام', price: '149', img: 'images/nescafe.jpg' },
    { title: 'مياه Nestle Pure Life 1.5 لتر × 12', price: '89', img: 'images/water.jpg' },
    { title: 'سامسونج جالاكسي S24 - 256 جيجا', price: '32,999', img: 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=300&q=80' },
    { title: 'أبل آيفون 15 - 128 جيجا', price: '45,999', img: 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=300&q=80' },
    { title: 'سامسونج جالاكسي A54 - 128 جيجا', price: '15,499', img: 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?auto=format&fit=crop&w=300&q=80' },
    { title: 'شاومي 14 Pro - 512 جيجا', price: '28,500', img: 'images/redmi.jpg' },
    { title: 'أوبو A78 - 256 جيجا، 8 رام', price: '10,999', img: 'https://images.unsplash.com/photo-1585060544812-6b45742d762f?auto=format&fit=crop&w=300&q=80' },
    { title: 'سامسونج جالاكسي تاب A8 - 64 جيجا', price: '11,999', img: 'https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=300&q=80' },
    { title: 'مجموعة LEGO City 300 قطعة - سيارات', price: '1,299', img: 'images/lego.jpg' },
    { title: 'عروسة Barbie Fashionista مع ملحقات', price: '599', img: 'images/barbie.jpg' },
    { title: 'روبوت ذكاء اصطناعي تعليمي للأطفال', price: '2,199', img: 'images/robot.jpg' },
    { title: 'دراجة هوائية للأطفال 16 بوصة', price: '1,499', img: 'images/bike.jpg' },
    { title: 'طقم رسم وتلوين احترافي للأطفال 48 قلم', price: '399', img: 'images/colors.jpg' },
    { title: 'حوض سباحة منزلي للأطفال 300×180 سم', price: '899', img: 'images/pool.jpg' },
    { title: 'ثلاجة توشيبا 20 قدم نوفروست - ستانلس', price: '22,500', img: 'https://images.unsplash.com/photo-1584568694244-14fbdf83bd30?auto=format&fit=crop&w=300&q=80' },
    { title: 'غسالة LG ذات حوضين 12 كيلو - أبيض', price: '14,999', img: 'https://images.unsplash.com/photo-1626806787461-102c1bfaaea1?auto=format&fit=crop&w=300&q=80' },
    { title: 'مكيف يونيون اير سبليت 1.5 حصان بارد', price: '13,500', img: 'images/ac.jpg' },
    { title: 'قلاية هوائية فيليبس XL - 6.2 لتر', price: '6,499', img: 'images/airfryer.jpg' },
    { title: 'طقم حلل جرانيت سافلون 9 قطع', price: '4,200', img: 'images/pots.jpg' },
    { title: 'مكنسة كهربائية Dyson V12 - لاسلكية', price: '18,999', img: 'images/vacuum.jpg' },
    { title: 'حذاء ركض Adidas Ultraboost - أسود', price: '5,200', img: 'images/shoes.jpg' },
    { title: 'تيشيرت رجالي Nike قطن 100% - أبيض', price: '599', img: 'images/tshirt.jpg' },
    { title: 'شنطة يد حريمي جلد طبيعي - بني', price: '2,999', img: 'images/bag.jpg' },
    { title: 'بنطلون جينز Levi\'s 501 - أزرق', price: '1,899', img: 'images/jeans.jpg' },
    { title: 'فستان سهرة حريمي - أحمر فاقع', price: '1,499', img: 'images/dress.jpg' },
    { title: 'ساعة Casio G-Shock رجالي - أسود', price: '3,200', img: 'images/watch.jpg' },
    { title: 'تلفزيون سامسونج QLED 55 بوصة 4K', price: '28,999', img: 'https://images.unsplash.com/photo-1593784991095-a205069470b6?auto=format&fit=crop&w=300&q=80' },
    { title: 'سماعات Apple AirPods Pro الجيل الثاني', price: '8,999', img: 'images/airpods.jpg' },
    { title: 'لاب توب لينوفو ايديا باد 3 - Intel i5', price: '22,999', img: 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=300&q=80' },
    { title: 'كاميرا Canon EOS R50 - 24 ميجابكسل', price: '19,500', img: 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=300&q=80' },
    { title: 'PlayStation 5 - 1TB SSD', price: '24,999', img: 'images/ps5.jpg' },
    { title: 'سماعة Sony WH-1000XM5 - ضد الضوضاء', price: '12,999', img: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=300&q=80' }
];

function handleSearch() {
    const searchInput = document.getElementById('main-search');
    if (!searchInput) return;
    const q = searchInput.value.trim().toLowerCase();
    
    const hero = document.querySelector('.hero-section');
    const banner = document.querySelector('.page-banner');
    const header = document.querySelector('.section-header h2');
    let grid = document.querySelector('.product-grid');
    
    // If no grid exists normally on this page (like cart), we don't show live results underneath
    if (!grid) return; 

    if (hero) hero.style.display = q ? 'none' : 'block';
    if (banner) banner.style.display = q ? 'none' : 'block';
    
    if (!q) {
        if (window.originalGridHTML) {
            grid.innerHTML = window.originalGridHTML;
            if (header) header.innerHTML = `<i class="fas fa-box"></i> المنتجات`;
            injectWishlistButtons();
            highlightWishlistedItems();
        }
        return;
    }
    
    // Save original layout
    if (!window.originalGridHTML) window.originalGridHTML = grid.innerHTML;
    
    // Find globally
    const matches = globalSearchDB.filter(p => p.title.toLowerCase().includes(q));
    
    if (header) {
        header.innerHTML = `<i class="fas fa-search"></i> نتائج البحث عن: "${searchInput.value}" (${matches.length})`;
    }
    
    if (matches.length === 0) {
        grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: #888;"><h3>لا توجد نتائج مطابقة لـ "${searchInput.value}"</h3><p>جرب كلمات مفتاحية تانية زي "ألوان" أو "بلايستيشن"</p></div>`;
        return;
    }
    
    let html = '';
    matches.forEach(p => {
        const numPrice = parseInt(p.price.replace(/[^\d]/g, '')) || 0;
        html += `
        <div class="product-card">
            <button class="wishlist-btn" onclick="toggleWishlist(this, '${p.title.replace(/'/g, "\\'")}', ${numPrice})" title="أضف للمفضلة"><i class="far fa-heart"></i></button>
            <a href="product-details.html">
                <div class="product-img-wrap"><img src="${p.img}" class="product-image" alt="Product"></div>
                <div class="product-info">
                    <h3>${p.title}</h3>
                    <div class="stars"><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="far fa-star"></i></div>
                    <div class="price-row"><div class="product-price">EGP ${p.price}</div></div>
                </div>
            </a>
            <button class="btn btn-orange" onclick="addToCart(this, '${p.title.replace(/'/g, "\\'")}', ${numPrice})" style="margin:8px; width:calc(100% - 16px); justify-content:center;"><i class="fas fa-shopping-cart"></i> أضف للسلة</button>
        </div>`;
    });
    
    grid.innerHTML = html;
    highlightWishlistedItems();
}
document.getElementById('main-search')?.addEventListener('keydown', e => { if (e.key === 'Enter') handleSearch(); });
// Add an input listener for real-time search
document.getElementById('main-search')?.addEventListener('input', handleSearch);

// ========== SLIDER ==========
let currentSlide = 0;
const slides = document.querySelectorAll('.slide');
const dots = document.querySelectorAll('.dot');
let autoSlideInterval;

function goToSlide(n) {
    if (!slides.length) return;
    slides[currentSlide].classList.remove('active');
    if (dots[currentSlide]) dots[currentSlide].classList.remove('active');
    currentSlide = (n + slides.length) % slides.length;
    slides[currentSlide].classList.add('active');
    if (dots[currentSlide]) dots[currentSlide].classList.add('active');
}
function nextSlide() { goToSlide(currentSlide + 1); }
function prevSlide() { goToSlide(currentSlide - 1); }
if (slides.length) {
    autoSlideInterval = setInterval(nextSlide, 4000);
    document.querySelector('.hero-slider')?.addEventListener('mouseenter', () => clearInterval(autoSlideInterval));
    document.querySelector('.hero-slider')?.addEventListener('mouseleave', () => { autoSlideInterval = setInterval(nextSlide, 4000); });
}

// ========== ORDERS ==========
function saveOrder(orderData) {
    const orders = JSON.parse(localStorage.getItem('jumia-orders') || '[]');
    orders.unshift(orderData);
    localStorage.setItem('jumia-orders', JSON.stringify(orders));
}

// ========== COUPON CODES ==========
const validCoupons = {
    'JUMIA10': { discount: 10, type: 'percent', label: 'خصم 10%' },
    'WELCOME50': { discount: 50, type: 'fixed', label: 'خصم 50 جنيه' },
    'SALE20': { discount: 20, type: 'percent', label: 'خصم 20%' },
    'SUMMER30': { discount: 30, type: 'percent', label: 'خصم 30%' },
};

let appliedCoupon = null;

function applyCoupon() {
    const code = document.getElementById('coupon-input')?.value.trim().toUpperCase();
    const msgEl = document.getElementById('coupon-msg');
    if (!code) return;

    if (validCoupons[code]) {
        appliedCoupon = { code, ...validCoupons[code] };
        if (msgEl) {
            msgEl.innerHTML = `<i class="fas fa-check-circle" style="color:#4caf50"></i> تم تطبيق الكوبون: ${appliedCoupon.label}`;
            msgEl.style.color = '#4caf50';
        }
        recalcCheckout();
        showToast(`✅ تم تطبيق كوبون ${code}`);
    } else {
        appliedCoupon = null;
        if (msgEl) {
            msgEl.innerHTML = `<i class="fas fa-times-circle"></i> كوبون غير صحيح`;
            msgEl.style.color = '#e53935';
        }
    }
}

function recalcCheckout() {
    const subtotal = cartItems.reduce((s, i) => s + i.price * i.qty, 0);
    let discount = 0;
    if (appliedCoupon) {
        if (appliedCoupon.type === 'percent') discount = Math.round(subtotal * appliedCoupon.discount / 100);
        else discount = appliedCoupon.discount;
    }
    const total = subtotal - discount;
    const subEl = document.getElementById('co-subtotal');
    const disEl = document.getElementById('co-discount');
    const totEl = document.getElementById('co-total');
    const disRow = document.getElementById('discount-row');
    if (subEl) subEl.textContent = subtotal.toLocaleString() + ' EGP';
    if (disEl && disRow) {
        if (discount > 0) {
            disEl.textContent = '-' + discount.toLocaleString() + ' EGP';
            disRow.style.display = 'flex';
        } else {
            disRow.style.display = 'none';
        }
    }
    if (totEl) totEl.textContent = total.toLocaleString() + ' EGP';
}

// ========== WISHLIST PAGE ==========
function renderWishlist() {
    const container = document.getElementById('wishlist-grid');
    if (!container) return;

    if (!wishlist.length) {
        container.innerHTML = `<div style="text-align:center;padding:80px;color:var(--muted);grid-column:1/-1;"><i class="fas fa-heart" style="font-size:60px;color:#eee;"></i><h3 style="margin-top:15px;">مفضلتك فارغة</h3><p>أضف منتجات للمفضلة عن طريق الضغط على ❤️</p><a href="index.html" class="btn btn-orange" style="margin-top:20px;display:inline-flex;">تصفح المنتجات</a></div>`;
        return;
    }

    container.innerHTML = wishlist.map((item, idx) => `
        <div class="product-card">
            <div class="product-img-wrap"><img src="${item.img || 'https://placehold.co/300x300'}" class="product-image" alt="${item.name}" onerror="this.src='https://placehold.co/300x300/f5f5f5/999?text=Item'"></div>
            <div class="product-info"><h3>${item.name}</h3><div class="product-price">EGP ${item.price.toLocaleString()}</div></div>
            <div style="display:flex;gap:8px;padding:8px;">
                <button class="btn btn-orange" style="flex:1;justify-content:center;" onclick="addWishToCart(${idx})"><i class="fas fa-shopping-cart"></i> أضف للسلة</button>
                <button class="btn btn-outline" style="padding:9px 12px;" onclick="removeWish(${idx})"><i class="fas fa-trash"></i></button>
            </div>
        </div>`).join('');
}

function addWishToCart(idx) {
    const item = wishlist[idx];
    const existing = cartItems.find(i => i.name === item.name);
    if (existing) existing.qty++;
    else cartItems.push({ name: item.name, price: item.price, qty: 1, img: item.img });
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    updateCartUI();
    showToast(`تمت الإضافة للسلة 🛒`);
}

function removeWish(idx) {
    wishlist.splice(idx, 1);
    localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
    renderWishlist();
    updateWishlistCount();
}

// ========== DYNAMIC PRODUCT DETAILS ==========
document.addEventListener('click', function(e) {
    const cardLink = e.target.closest('.product-card a');
    if (cardLink && cardLink.getAttribute('href') === 'product-details.html') {
        const card = cardLink.closest('.product-card');
        const img = card.querySelector('img.product-image')?.src;
        const title = card.querySelector('h3')?.textContent;
        const price = card.querySelector('.product-price')?.textContent;
        const oldPrice = card.querySelector('.product-old-price')?.textContent || '';
        
        if (img && title) {
            localStorage.setItem('jumia-current-product', JSON.stringify({ img, title, price, oldPrice }));
        }
    }
});

function loadProductDetails() {
    if (window.location.pathname.includes('product-details.html')) {
        const prodData = JSON.parse(localStorage.getItem('jumia-current-product'));
        if (prodData) {
            const mainImg = document.getElementById('main-product-img');
            const titleEl = document.querySelector('.product-meta h1');
            const priceEl = document.querySelector('.detail-price');
            const oldPriceEl = document.querySelector('.detail-old-price');
            const addBtn = document.getElementById('add-cart-btn');
            
            const thumbGrid = document.querySelector('.thumb-grid');
            if (thumbGrid) thumbGrid.style.display = 'none';
            
            if (mainImg) mainImg.src = prodData.img;
            if (titleEl) titleEl.textContent = prodData.title;
            if (priceEl) priceEl.textContent = prodData.price;
            if (oldPriceEl) oldPriceEl.textContent = prodData.oldPrice;
            
            if (addBtn) {
                const numPrice = parseInt(prodData.price.replace(/[^\d]/g, '')) || 0;
                addBtn.setAttribute('onclick', `addToCart(this, '${prodData.title.replace(/'/g, "\\'")}', ${numPrice})`);
            }
            
            const bc = document.querySelector('.breadcrumb');
            if (bc) {
                bc.innerHTML = `<a href="index.html">الرئيسية</a><span>›</span> ${prodData.title}`;
            }
            
            // Also update page title
            document.title = `${prodData.title} | جوميا مصر`;
        }
    }
}

// ========== INJECT WISHLIST BUTTONS ==========
function injectWishlistButtons() {
    document.querySelectorAll('.product-card').forEach(card => {
        if (!card.querySelector('.wishlist-btn')) {
            const title = card.querySelector('h3')?.textContent || '';
            const priceText = card.querySelector('.product-price')?.textContent || '0';
            const numPrice = parseInt(priceText.replace(/[^\d]/g, '')) || 0;
            
            const btn = document.createElement('button');
            btn.className = 'wishlist-btn';
            btn.title = 'أضف للمفضلة';
            btn.setAttribute('onclick', `toggleWishlist(this, '${title.replace(/'/g, "\\'")}', ${numPrice})`);
            btn.innerHTML = '<i class="far fa-heart"></i>';
            card.prepend(btn);
        }
    });
}

// ========== INIT ==========
updateCartUI();
updateWishlistCount();
updateHeaderUser();
injectWishlistButtons();
highlightWishlistedItems();
renderCart();
renderWishlist();
loadProductDetails();
