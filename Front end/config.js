/* ============================================
   Front-end configuration
   Single source of truth for the API base URL.
   Change it here once instead of in every file.
============================================ */
// Local backend (emails/2FA on jumiaapi.runasp.net are broken — use local API).
const API_BASE_URL = 'http://localhost:5208/api';
// Remote (only after backend team fixes SMTP / disables 2FA):
// const API_BASE_URL = 'http://jumiaapi.runasp.net/api';

/** Resolve product image paths coming from the API (relative or absolute). */
function resolveImageUrl(url) {
    if (!url) return 'https://placehold.co/300x300/f5f5f5/999?text=Product';
    if (/^https?:\/\//i.test(url)) return url;
    return url; // relative paths like images/ps5.jpg resolve against the front-end root
}

/** Read a field that may be camelCase or PascalCase (ASP.NET responses vary). */
function apiField(obj, name) {
    if (!obj) return undefined;
    if (obj[name] !== undefined) return obj[name];
    const pascal = name.charAt(0).toUpperCase() + name.slice(1);
    return obj[pascal];
}

/** Auth header helper */
function authHeaders(extra = {}) {
    const headers = { ...extra };
    if (typeof currentUser !== 'undefined' && currentUser && currentUser.token) {
        headers['Authorization'] = `Bearer ${currentUser.token}`;
    }
    return headers;
}

/** Quick health check — returns true if the live API is reachable as JSON */
async function isApiOnline() {
    try {
        const res = await fetch(`${API_BASE_URL}/Products?pageSize=1`, { cache: 'no-store' });
        if (!res.ok) return false;
        const ct = res.headers.get('content-type') || '';
        if (!ct.includes('application/json')) return false;
        await res.json();
        return true;
    } catch (e) {
        return false;
    }
}

/**
 * Map front-end pages to category names / product-name filters.
 * Live DB may tag Arabic catalog items under Electronics, so we also
 * filter by known product name prefixes when needed.
 */
const PAGE_PRODUCT_FILTERS = {
    'mobiles.html': {
        categoryNames: ['Smartphones'],
        nameIncludes: ['جالاكسي S24', 'آيفون 15', 'جالاكسي A54', 'شاومي', 'أوبو', 'جالاكسي تاب', 'iPhone', 'Galaxy S24']
    },
    'electronics.html': {
        categoryNames: ['Electronics', 'Laptops'],
        nameIncludes: ['تلفزيون', 'AirPods', 'لاب توب', 'كاميرا', 'PlayStation', 'سماعة Sony', 'Sony WH']
    },
    'home-appliances.html': {
        categoryNames: ['Home Appliances', 'Home & Kitchen'],
        nameIncludes: ['ثلاجة', 'غسالة', 'مكيف', 'قلاية', 'حلل', 'مكنسة', 'Dyson', 'Airfryer']
    },
    'fashion.html': {
        categoryNames: ['Fashion', 'Shoes', 'Clothing'],
        nameIncludes: ['حذاء', 'تيشيرت', 'شنطة', 'جينز', 'فستان', 'ساعة Casio', 'Adidas', 'Nike']
    },
    'supermarket.html': {
        categoryNames: ['Supermarket'],
        nameIncludes: ['زيت زيتون', 'لبن', 'شوكولاتة', 'مسحوق', 'قهوة', 'مياه', 'Ferrero', 'Ariel', 'Nescafé', 'Nestle']
    },
    'kids.html': {
        categoryNames: ['Kids'],
        nameIncludes: ['LEGO', 'Barbie', 'روبوت', 'دراجة', 'تلوين', 'حوض سباحة', 'ألعاب']
    }
};
