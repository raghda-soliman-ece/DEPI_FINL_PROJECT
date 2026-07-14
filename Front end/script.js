/* ============================================
   JUMIA EGYPT - Main Script
   Features: Cart, Wishlist, User Session, Coupons, Orders
   Connected to: http://jumiaapi.runasp.net/api
============================================ */

// ========== USER SESSION ==========
let currentUser = JSON.parse(localStorage.getItem('jumia-user') || 'null');

function updateHeaderUser() {
    const loginLinks = document.querySelectorAll('.login-link');

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

function productCardHtml(p) {
    const name = (p.name || '').replace(/'/g, "\\'");
    const price = Number(p.price) || 0;
    const img = resolveImageUrl(p.pictureUrl);
    const oldPrice = p.oldPrice ? Number(p.oldPrice) : null;
    const discount = oldPrice && oldPrice > price
        ? Math.round((1 - price / oldPrice) * 100)
        : null;

    return `
        <div class="product-card" data-product-id="${p.id}">
            <button class="wishlist-btn" onclick="toggleWishlist(this, '${name}', ${price}, ${p.id})" title="أضف للمفضلة"><i class="far fa-heart"></i></button>
            <a href="product-details.html?id=${p.id}">
                <div class="product-img-wrap">
                    <img src="${img}" class="product-image" alt="${p.name || ''}" onerror="this.src='https://placehold.co/300x300/f5f5f5/999?text=Product'">
                </div>
                <div class="product-info">
                    <h3>${p.name || ''}</h3>
                    <div class="product-price">EGP ${price.toLocaleString()}</div>
                    ${oldPrice ? `<div class="price-row"><span class="product-old-price">${oldPrice.toLocaleString()} EGP</span>${discount ? `<span class="product-discount">-${discount}%</span>` : ''}</div>` : ''}
                    <div class="stars"><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="fas fa-star"></i><i class="far fa-star"></i></div>
                </div>
            </a>
            <button class="btn btn-orange" onclick="addToCart(this, '${name}', ${price}, ${p.id})" style="margin:8px; width:calc(100% - 16px); justify-content:center;"><i class="fas fa-shopping-cart"></i> اضف للسلة</button>
        </div>`;
}

function dedupeProductsByName(products) {
    const map = new Map();
    products.forEach(p => {
        const key = (p.name || '').trim();
        if (!key) return;
        const prev = map.get(key);
        if (!prev || (p.id || 0) > (prev.id || 0)) map.set(key, p);
    });
    return Array.from(map.values());
}

async function fetchProducts(params = {}) {
    const qs = new URLSearchParams({ pageSize: 100, pageIndex: 1, ...params });
    const res = await fetch(`${API_BASE_URL}/Products?${qs}`);
    if (!res.ok) throw new Error('API down');
    const data = await res.json();
    return Array.isArray(data) ? data : (data.data || data.Data || []);
}

async function addToCart(btn, name, price, forceId = null) {
    const imgEl = btn.closest('.product-card')?.querySelector('img');
    const existing = cartItems.find(i => (forceId && i.productId === forceId) || i.name === name);

    if (existing) {
        existing.qty++;
        if (forceId) existing.productId = forceId;
    } else {
        cartItems.push({ name, price, qty: 1, img: imgEl?.src || '', productId: forceId });
    }
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    updateCartUI();

    if (btn && btn.innerHTML !== undefined) {
        const orig = btn.innerHTML;
        btn.innerHTML = '<i class="fas fa-check"></i> تم الإضافة!';
        btn.style.background = '#4caf50';
        setTimeout(() => { btn.innerHTML = orig; btn.style.background = ''; }, 1500);
    }
    showToast(`تمت إضافة "${name}" للسلة 🛒`);

    if (currentUser && currentUser.token) {
        try {
            let pId = forceId || existing?.productId;
            if (!pId) {
                const products = await fetchProducts({ search: name });
                const matched = products.find(p => p.name === name);
                if (matched) pId = matched.id;
            }
            const item = cartItems.find(i => i.name === name || (pId && i.productId === pId));
            if (pId && item) {
                item.productId = pId;
                localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
                // Backend replaces quantity with the value sent
                await fetch(`${API_BASE_URL}/Basket/items?productId=${pId}&quantity=${item.qty}`, {
                    method: 'POST',
                    headers: authHeaders()
                });
            }
        } catch (err) {
            console.error('Failed to sync cart item to backend.', err);
        }
    }
}

// ========== WISHLIST ==========
let wishlist = JSON.parse(localStorage.getItem('jumia-wishlist') || '[]');

function toggleWishlist(btn, name, price, forceId = null) {
    const imgEl = btn.closest('.product-card')?.querySelector('img');
    const idx = wishlist.findIndex(i => (forceId && i.productId === forceId) || i.name === name);
    if (idx === -1) {
        wishlist.push({ name, price, img: imgEl?.src || '', productId: forceId });
        btn.classList.add('wishlisted');
        btn.innerHTML = '<i class="fas fa-heart"></i>';
        showToast(`تمت الإضافة للمفضلة ❤️`);

        if (currentUser && currentUser.token && forceId) {
            fetch(`${API_BASE_URL}/Wishlist/${forceId}`, {
                method: 'POST',
                headers: authHeaders()
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
                headers: authHeaders()
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
            <img src="${resolveImageUrl(item.img) || 'https://placehold.co/90x90'}" alt="${item.name}" onerror="this.src='https://placehold.co/90x90/f5f5f5/999?text=Item'">
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
            await fetch(`${API_BASE_URL}/Basket/items?productId=${item.productId}&quantity=${item.qty}`, {
                method: 'POST',
                headers: authHeaders()
            });
        } catch (err) {
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
                headers: authHeaders()
            });
        } catch (err) {
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
                    headers: authHeaders()
                });
            } catch (err) {}
        }
    }
    await removeItem(idx);
    showToast('تم نقل المنتج للمفضلة ❤️');
}

/** Push local cart to backend basket before creating an order */
async function syncCartToBackend() {
    if (!currentUser?.token) return false;
    for (const item of cartItems) {
        if (!item.productId) {
            try {
                const products = await fetchProducts({ search: item.name });
                const matched = products.find(p => p.name === item.name);
                if (matched) item.productId = matched.id;
            } catch (e) {}
        }
        if (item.productId) {
            await fetch(`${API_BASE_URL}/Basket/items?productId=${item.productId}&quantity=${item.qty}`, {
                method: 'POST',
                headers: authHeaders()
            });
        }
    }
    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
    return true;
}

// ========== SEARCH ==========
async function handleSearch() {
    const searchInput = document.getElementById('main-search');
    if (!searchInput) return;
    const q = searchInput.value.trim();

    const hero = document.querySelector('.hero-section');
    const banner = document.querySelector('.page-banner');
    const header = document.querySelector('.section-header h2');
    let grid = document.querySelector('.product-grid');

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

    if (!window.originalGridHTML) window.originalGridHTML = grid.innerHTML;

    let matches = [];
    try {
        matches = dedupeProductsByName(await fetchProducts({ search: q }));
    } catch (err) {
        console.warn('Search failed', err);
        matches = [];
    }

    if (header) {
        header.innerHTML = `<i class="fas fa-search"></i> نتائج البحث عن: "${searchInput.value}" (${matches.length})`;
    }

    if (matches.length === 0) {
        grid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: #888;"><h3>لا توجد نتائج مطابقة لـ "${searchInput.value}"</h3><p>جرب كلمات مفتاحية تانية</p></div>`;
        return;
    }

    grid.innerHTML = matches.map(productCardHtml).join('');
    highlightWishlistedItems();
}
document.getElementById('main-search')?.addEventListener('keydown', e => { if (e.key === 'Enter') handleSearch(); });
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
            <div class="product-img-wrap"><img src="${resolveImageUrl(item.img)}" class="product-image" alt="${item.name}" onerror="this.src='https://placehold.co/300x300/f5f5f5/999?text=Item'"></div>
            <div class="product-info"><h3>${item.name}</h3><div class="product-price">EGP ${Number(item.price).toLocaleString()}</div></div>
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
                headers: authHeaders()
            });
        } catch (err) {}
    }
}

// ========== DYNAMIC PRODUCT DETAILS ==========
document.addEventListener('click', function (e) {
    const cardLink = e.target.closest('.product-card a');
    if (cardLink && (cardLink.getAttribute('href') || '').includes('product-details.html')) {
        const card = cardLink.closest('.product-card');
        const img = card.querySelector('img.product-image')?.src;
        const title = card.querySelector('h3')?.textContent;
        const price = card.querySelector('.product-price')?.textContent;
        const oldPrice = card.querySelector('.product-old-price')?.textContent || '';
        const productId = card.dataset.productId || null;

        if (img && title) {
            localStorage.setItem('jumia-current-product', JSON.stringify({ img, title, price, oldPrice, id: productId }));
        }
    }
});

async function loadProductDetails() {
    if (!window.location.pathname.includes('product-details.html') && !window.location.href.includes('product-details.html')) return;

    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');
    let prodData = JSON.parse(localStorage.getItem('jumia-current-product') || 'null');

    if (id) {
        try {
            const res = await fetch(`${API_BASE_URL}/Products/${id}`);
            if (res.ok) {
                const p = await res.json();
                prodData = {
                    id: p.id,
                    img: resolveImageUrl(p.pictureUrl),
                    title: p.name,
                    price: `EGP ${(p.price || 0).toLocaleString()}`,
                    oldPrice: p.oldPrice ? `${Number(p.oldPrice).toLocaleString()} EGP` : '',
                    description: p.description || ''
                };
                localStorage.setItem('jumia-current-product', JSON.stringify(prodData));
            }
        } catch (err) {
            console.warn('Failed to load product by id', err);
        }
    }

    if (!prodData) return;

    const mainImg = document.getElementById('main-product-img');
    const titleEl = document.querySelector('.product-meta h1');
    const priceEl = document.querySelector('.detail-price');
    const oldPriceEl = document.querySelector('.detail-old-price');
    const addBtn = document.getElementById('add-cart-btn');
    const descEl = document.querySelector('.product-meta p') || document.querySelector('.product-description');

    const thumbGrid = document.querySelector('.thumb-grid');
    if (thumbGrid) thumbGrid.style.display = 'none';

    if (mainImg) mainImg.src = resolveImageUrl(prodData.img);
    if (titleEl) titleEl.textContent = prodData.title;
    if (priceEl) priceEl.textContent = prodData.price;
    if (oldPriceEl) oldPriceEl.textContent = prodData.oldPrice;
    if (descEl && prodData.description) descEl.textContent = prodData.description;

    if (addBtn) {
        const numPrice = parseInt(String(prodData.price).replace(/[^\d]/g, ''), 10) || 0;
        const safeTitle = String(prodData.title || '').replace(/'/g, "\\'");
        const pid = prodData.id ? `, ${prodData.id}` : '';
        addBtn.setAttribute('onclick', `addToCart(this, '${safeTitle}', ${numPrice}${pid})`);
    }

    const bc = document.querySelector('.breadcrumb');
    if (bc) {
        bc.innerHTML = `<a href="index.html">الرئيسية</a><span>›</span> ${prodData.title}`;
    }

    document.title = `${prodData.title} | جوميا مصر`;
}

// ========== INJECT WISHLIST BUTTONS ==========
function injectWishlistButtons() {
    document.querySelectorAll('.product-card').forEach(card => {
        if (!card.querySelector('.wishlist-btn')) {
            const title = card.querySelector('h3')?.textContent || '';
            const priceText = card.querySelector('.product-price')?.textContent || '0';
            const numPrice = parseInt(priceText.replace(/[^\d]/g, ''), 10) || 0;
            const pid = card.dataset.productId ? `, ${card.dataset.productId}` : '';

            const btn = document.createElement('button');
            btn.className = 'wishlist-btn';
            btn.title = 'أضف للمفضلة';
            btn.setAttribute('onclick', `toggleWishlist(this, '${title.replace(/'/g, "\\'")}', ${numPrice}${pid})`);
            btn.innerHTML = '<i class="far fa-heart"></i>';
            card.prepend(btn);
        }
    });
}

// ========== DYNAMIC API FUNCTIONS ==========
async function loadFeaturedProducts() {
    // Home page has the hero slider; category pages use page-banner instead
    if (!document.querySelector('.hero-section')) return;

    const grid = document.querySelector('.products-section .product-grid');
    if (!grid) return;

    try {
        const products = dedupeProductsByName(await fetchProducts({ pageSize: 12 }));
        if (products.length > 0) {
            grid.innerHTML = products.slice(0, 12).map(productCardHtml).join('');
            window.originalGridHTML = grid.innerHTML;
            highlightWishlistedItems();
        }
    } catch (err) {
        console.error('Failed to load featured products', err);
    }
}

async function loadCategoryProducts() {
    const page = (window.location.pathname.split('/').pop() || '').toLowerCase();
    const filter = typeof PAGE_PRODUCT_FILTERS !== 'undefined' ? PAGE_PRODUCT_FILTERS[page] : null;
    if (!filter) return;

    const grid = document.querySelector('.products-section .product-grid');
    if (!grid) return;

    const fallbackHtml = grid.innerHTML;
    grid.innerHTML = `<div style="grid-column:1/-1;text-align:center;padding:40px;color:#888;"><i class="fas fa-spinner fa-spin" style="font-size:28px;color:var(--primary);"></i><p style="margin-top:12px;">جاري تحميل المنتجات...</p></div>`;

    try {
        let products = [];

        // 1) Try matching category names from API
        try {
            const catRes = await fetch(`${API_BASE_URL}/Categories`);
            if (catRes.ok) {
                const cats = await catRes.json();
                const wanted = cats.filter(c =>
                    filter.categoryNames.some(n => (c.name || '').toLowerCase() === n.toLowerCase())
                );
                for (const cat of wanted) {
                    const batch = await fetchProducts({ categoryId: cat.id, pageSize: 50 });
                    products.push(...batch);
                }
            }
        } catch (e) {}

        // 2) Fallback / supplement: name keyword filter across catalog
        if (products.length < 4) {
            const all = await fetchProducts({ pageSize: 100 });
            const byName = all.filter(p =>
                filter.nameIncludes.some(kw => (p.name || '').includes(kw))
            );
            products = products.concat(byName);
        }

        products = dedupeProductsByName(products);

        if (products.length > 0) {
            grid.innerHTML = products.map(productCardHtml).join('');
            window.originalGridHTML = grid.innerHTML;
            highlightWishlistedItems();
        } else {
            grid.innerHTML = fallbackHtml;
            window.originalGridHTML = fallbackHtml;
            injectWishlistButtons();
            highlightWishlistedItems();
        }
    } catch (err) {
        console.error('Failed to load category products', err);
        grid.innerHTML = fallbackHtml;
        injectWishlistButtons();
        highlightWishlistedItems();
    }
}

async function loadCartFromBackend() {
    if (currentUser && currentUser.token) {
        try {
            const res = await fetch(`${API_BASE_URL}/Basket`, {
                headers: authHeaders()
            });
            if (res.ok) {
                const data = await res.json();
                const items = data.items || data.Items;
                if (items && items.length) {
                    cartItems = items.map(i => ({
                        name: apiField(i, 'productName'),
                        price: Number(apiField(i, 'productPrice')) || 0,
                        qty: apiField(i, 'quantity') || 1,
                        img: resolveImageUrl(apiField(i, 'productPictureUrl')),
                        productId: apiField(i, 'productId')
                    }));
                    localStorage.setItem('jumia-cart', JSON.stringify(cartItems));
                    updateCartUI();
                    renderCart();
                }
            }
        } catch (err) {
            console.error('Failed to load cart from backend');
        }
    }
}

async function loadWishlistFromBackend() {
    if (currentUser && currentUser.token) {
        try {
            const res = await fetch(`${API_BASE_URL}/Wishlist`, {
                headers: authHeaders()
            });
            if (res.ok) {
                const data = await res.json();
                const list = Array.isArray(data) ? data : [];
                wishlist = list.map(i => ({
                    name: apiField(i, 'productName'),
                    price: Number(apiField(i, 'productPrice')) || 0,
                    img: resolveImageUrl(apiField(i, 'productPictureUrl')),
                    productId: apiField(i, 'productId')
                }));
                localStorage.setItem('jumia-wishlist', JSON.stringify(wishlist));
                updateWishlistCount();
                renderWishlist();
                highlightWishlistedItems();
            }
        } catch (err) {
            console.error('Failed to load wishlist from backend');
        }
    }
}

async function loadOrdersFromBackend() {
    if (!currentUser?.token) return null;
    try {
        const res = await fetch(`${API_BASE_URL}/Orders`, { headers: authHeaders() });
        if (!res.ok) return null;
        return await res.json();
    } catch (e) {
        return null;
    }
}

// ========== INIT ==========
(async function init() {
    updateCartUI();
    updateWishlistCount();
    updateHeaderUser();
    injectWishlistButtons();
    highlightWishlistedItems();
    renderCart();
    renderWishlist();

    const online = await isApiOnline();
    if (!online) {
        console.warn('Live API unreachable from this network:', API_BASE_URL);
        // Site still works with local cart / static products
    }

    await Promise.all([
        loadFeaturedProducts(),
        loadCategoryProducts(),
        loadCartFromBackend(),
        loadWishlistFromBackend(),
        loadProductDetails()
    ]);
})();
