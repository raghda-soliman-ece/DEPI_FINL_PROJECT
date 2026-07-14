const fs = require('fs');
const files = fs.readdirSync('.').filter(f => f.endsWith('.html'));

const headerTarget = '<div class="user-actions"><a href="login.html" class="action-link"><i class="far fa-user"></i><span>تسجيل الدخول</span></a><a href="cart.html" class="action-link"><div class="cart-icon-wrapper"><i class="fas fa-shopping-cart"></i><span class="cart-count">0</span></div><span>السلة</span></a></div>';

const headerReplacement = '<div class="user-actions"><a href="login.html" class="action-link login-link" id="login-header-link"><i class="far fa-user"></i><span>تسجيل الدخول</span></a><a href="account.html" class="action-link" id="wishlist-header-link"><div class="cart-icon-wrapper"><i class="far fa-heart"></i><span class="wishlist-count">0</span></div><span>مفضلتي</span></a><a href="cart.html" class="action-link"><div class="cart-icon-wrapper"><i class="fas fa-shopping-cart"></i><span class="cart-count">0</span></div><span>السلة</span></a></div>';

for (let file of files) {
  if (file === 'index.html' || file === 'account.html') continue;
  let content = fs.readFileSync(file, 'utf8');
  let changed = false;
  
  if (content.includes(headerTarget)) {
    content = content.replace(headerTarget, headerReplacement);
    changed = true;
  }

  // Add wishlist buttons
  // Look for product-card
  const regex = /<div class="product-card">(?!<button class="wishlist-btn")(<a href="[^"]+">.*?)<button class="btn btn-orange(?: add-to-cart)?" onclick="[^"]*addToCart\(this,\s*'([^']+)',\s*(\d+)\)[^"]*">/gs;
  if(regex.test(content)) {
      content = content.replace(regex, '<div class="product-card"><button class="wishlist-btn" onclick="toggleWishlist(this, \'$2\', $3)" title="أضف للمفضلة"><i class="far fa-heart"></i></button>$1<button class="btn btn-orange add-to-cart" onclick="addToCart(this, \'$2\', $3)">');
      changed = true;
  }

  if (changed) {
    fs.writeFileSync(file, content, 'utf8');
    console.log('Updated ' + file);
  }
}
