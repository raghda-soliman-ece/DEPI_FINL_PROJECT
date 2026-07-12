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

async function addToCart(btn, name, price, forceId = null) {
    const imgEl = btn.closest('.product-card')?.querySelector('img');
    const existing = cartItems.find(i => i.name === name);
    
    // Immediate UI update
    if (existing) {
        existing.qty++;
    } else {
        cartItems.push({ name, price, qty: 1, img: imgEl?.src || '', productId: forceId });
    }
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    updateCartUI();
    
    const orig = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-check"></i> تم الإضافة!';
    btn.style.background = '#4caf50';
    setTimeout(() => { btn.innerHTML = orig; btn.style.background = ''; }, 1500);
    showToast(`تمت إضافة "${name}" للسلة 🛒`);

    if (currentUser && currentUser.token) {
        try {
            let pId = forceId;
            if (!pId) {
                // If ID is missing, try to find it via search API
                const sRes = await fetch(`${API_BASE_URL}/Products?search=${encodeURIComponent(name)}`);
                const sData = await sRes.json();
                const matched = (sData.data || sData || []).find(p => p.name === name);
                if (matched) pId = matched.id;
            }
            if (pId) {
                await fetch(`${API_BASE_URL}/Basket/items?productId=${pId}&quantity=1`, {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${currentUser.token}` }
                });
            }
        } catch(err) {
            console.error("Failed to sync cart item to backend.", err);
        }
    }
}

// ========== WISHLIST ==========
let wishlist = JSON.parse(localStorage.getItem('jumia-wishlist') || '[]');

function toggleWishlist(btn, name, price, forceId = null) {
    const imgEl = btn.closest('.product-card')?.querySelector('img');
    const idx = wishlist.findIndex(i => i.name === name);
    if (idx === -1) {
        wishlist.push({ name, price, img: imgEl?.src || '', productId: forceId });
        btn.classList.add('wishlisted');
        btn.innerHTML = '<i class="fas fa-heart"></i>';
        showToast(`تمت الإضافة للمفضلة ❤️`);
        
        if (currentUser && currentUser.token && forceId) {
            fetch(`${API_BASE_URL}/Wishlist/${forceId}`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            }).catch(e => console.error(e));
        }
    } else {
        const removedItem = wishlist.splice(idx, 1)[0];
        btn.classList.remove('wishlisted');
        btn.innerHTML = '<i class="far fa-heart"></i>';
        showToast(`تمت الإزالة من المفضلة`);
        
        const pId = removedItem.productId || forceId;
        if (currentUser && currentUser.token && pId) {
            fetch(`${API_BASE_URL}/Wishlist/${pId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            }).catch(e => console.error(e));
        }
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

async function changeQty(idx, delta) {
    const item = cartItems[idx];
    item.qty += delta;
    
    if (item.qty <= 0) {
        return removeItem(idx);
    }
    
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    renderCart(); updateCartUI();
    
    if (currentUser && currentUser.token && item.productId) {
        try {
            // Delete and re-add to ensure exact quantity since we don't have a PUT or PATCH
            await fetch(`${API_BASE_URL}/Basket/items/${item.productId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });
            await fetch(`${API_BASE_URL}/Basket/items?productId=${item.productId}&quantity=${item.qty}`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });
        } catch(err) {
            console.error('Failed to sync qty to backend');
        }
    }
}

async function removeItem(idx) {
    const item = cartItems[idx];
    cartItems.splice(idx, 1);
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    renderCart(); updateCartUI();
    
    if (currentUser && currentUser.token && item.productId) {
        try {
            await fetch(`${API_BASE_URL}/Basket/items/${item.productId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });
        } catch(err) {
            console.error('Failed to remove item from backend');
        }
    }
}

async function moveToWishlist(idx) {
    const item = cartItems[idx];
    if (!wishlist.find(w => w.name === item.name)) {
        wishlist.push({ name: item.name, price: item.price, img: item.img, productId: item.productId });
        localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
        updateWishlistCount();
        
        if (currentUser && currentUser.token && item.productId) {
            try {
                await fetch(`${API_BASE_URL}/Wishlist/${item.productId}`, {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${currentUser.token}` }
                });
            } catch(err) {}
        }
    }
    await removeItem(idx);
    showToast('تم نقل المنتج للمفضلة ❤️');
}

// ========== SEARCH ==========
// globalSearchDB removed in favor of API

const API_BASE_URL = 'http://jumiaapi.runasp.net/api';

async function handleSearch() {
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
    
    let matches = [];
    
    try {
        const res = await fetch(`${API_BASE_URL}/Products?search=${encodeURIComponent(q)}`);
        if (!res.ok) throw new Error('API down');
        let data = await res.json();
        if (data && data.data) data = data.data; // Handle pagination wrapper if any
        matches = data.map(p => ({
            id: p.id,
            title: p.name,
            price: p.price.toString(),
            img: p.pictureUrl || 'https://placehold.co/300x300'
        }));
    } catch (err) {
        console.warn("Search failed", err);
        matches = [];
    }
    
    if (header) {
        header.innerHTML = `<i class="fas fa-search"></i> نتائج البحث عن: "${searchInput.value}" (${matches.length})`;
    }
    
    if (matches.length === 0) {
        grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: #888;"><h3>لا توجد نتائج مطابقة لـ "${searchInput.value}"</h3><p>جرب كلمات مفتاحية تانية زي "ألوان" أو "بلايستيشن"</p></div>`;
        return;
    }
    
    let html = '';
    matches.forEach(p => {
        const numPrice = parseInt(p.price.replace(/[^\\d]/g, '')) || 0;
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

async function addWishToCart(idx) {
    const item = wishlist[idx];
    const dummyBtn = document.createElement('button');
    await addToCart(dummyBtn, item.name, item.price, item.productId);
}

async function removeWish(idx) {
    const item = wishlist[idx];
    wishlist.splice(idx, 1);
    localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
    renderWishlist();
    updateWishlistCount();
    
    if (currentUser && currentUser.token && item.productId) {
        try {
            await fetch(`${API_BASE_URL}/Wishlist/${item.productId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });
        } catch(err) {}
    }
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

// ========== DYNAMIC API FUNCTIONS ==========
async function loadFeaturedProducts() {
    // Only run on the home page
    const isHomePage = window.location.pathname.endsWith('index.html') || window.location.pathname.endsWith('/') || !window.location.pathname.includes('.html');
    if (!isHomePage) return;

    const grid = document.querySelector('.products-section .product-grid');
    if (!grid) return;
    
    try {
        const res = await fetch(`${API_BASE_URL}/Products?pageSize=6`);
        if (!res.ok) throw new Error('API down');
        let data = await res.json();
        const products = Array.isArray(data) ? data : (data.data || []);
        
        if (products.length > 0) {
            let html = '';
            products.forEach(p => {
                html += `
                <div class="product-card">
                    <button class="wishlist-btn" onclick="toggleWishlist(this, '${p.name.replace(/'/g, "\\'")}', ${p.price}, ${p.id})" title="أضف للمفضلة"><i class="far fa-heart"></i></button>
                    <a href="product-details.html">
                        <div class="product-img-wrap">
                            <img src="${p.pictureUrl || 'https://placehold.co/300x300'}" class="product-image" alt="Product">
                        </div>
                        <div class="product-info">
                            <h3>${p.name}</h3>
                            <div class="product-price">EGP ${p.price.toLocaleString()}</div>
                            ${p.oldPrice ? `<div class="price-row"><span class="product-old-price">${p.oldPrice.toLocaleString()} EGP</span></div>` : ''}
                            <div class="stars"><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="far fa-star"></i></div>
                        </div>
                    </a>
                    <button class="btn btn-orange" onclick="addToCart(this, '${p.name.replace(/'/g, "\\'")}', ${p.price}, ${p.id})" style="margin:8px; width:calc(100% - 16px); justify-content:center;"><i class="fas fa-shopping-cart"></i> اضف للسلة</button>
                </div>`;
            });
            grid.innerHTML = html;
            window.originalGridHTML = grid.innerHTML;
            highlightWishlistedItems();
        }
    } catch(err) {
        console.error("Failed to load featured products", err);
    }
}

async function loadCartFromBackend() {
    if (currentUser && currentUser.token) {
        try {
            const res = await fetch(`${API_BASE_URL}/Basket`, {
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });
            if (res.ok) {
                const data = await res.json();
                if (data.items) {
                    cartItems = data.items.map(i => ({
                        name: i.productName,
                        price: i.productPrice,
                        qty: i.quantity,
                        img: i.productPictureUrl || 'https://placehold.co/90x90',
                        productId: i.productId
                    }));
                    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
                    updateCartUI();
                    renderCart();
                }
            }
        } catch(err) {
            console.error('Failed to load cart from backend');
        }
    }
}

async function loadWishlistFromBackend() {
    if (currentUser && currentUser.token) {
        try {
            const res = await fetch(`${API_BASE_URL}/Wishlist`, {
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });
            if (res.ok) {
                const data = await res.json();
                wishlist = data.map(i => ({
                    name: i.productName,
                    price: i.productPrice,
                    img: i.productPictureUrl || 'https://placehold.co/300x300',
                    productId: i.productId
                }));
                localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
                updateWishlistCount();
                renderWishlist();
                highlightWishlistedItems();
            }
        } catch(err) {
            console.error('Failed to load wishlist from backend');
        }
    }
}

// ========== INIT ==========
loadFeaturedProducts();
loadCartFromBackend();
loadWishlistFromBackend();
loadProductDetails();
updateCartUI();
updateWishlistCount();
updateHeaderUser();
injectWishlistButtons();
highlightWishlistedItems();
renderCart();
renderWishlist();
