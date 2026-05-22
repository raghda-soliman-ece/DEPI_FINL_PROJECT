const fs = require('fs');
const files = ['mobiles.html', 'electronics.html', 'home-appliances.html', 'fashion.html', 'supermarket.html', 'kids.html'];
let prods = [];
for (let f of files) {
    if (fs.existsSync(f)) {
        let code = fs.readFileSync(f, 'utf8');
        let regex = /<img src="([^"]+)"[^>]*class="product-image"[^>]*>.*?<h3>([^<]+)<\/h3>.*?<div class="product-price">EGP ([^<]+)<\/div>/gs;
        let m;
        while ((m = regex.exec(code)) !== null) {
            prods.push({ img: m[1], title: m[2].trim(), price: m[3].trim() });
        }
    }
}
fs.writeFileSync('all_products.json', JSON.stringify(prods, null, 2));
