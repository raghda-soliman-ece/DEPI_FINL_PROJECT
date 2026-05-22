import os
import re
import json

files = ['mobiles.html', 'electronics.html', 'home-appliances.html', 'fashion.html', 'supermarket.html', 'kids.html']
prods = []

for f in files:
    if os.path.exists(f):
        with open(f, encoding='utf-8') as file:
            content = file.read()
            # regex to find image, title, and price
            # assuming structure: <img src="images/XXX" ...> ... <h3>TITLE</h3> ... <div class="product-price">EGP PRICE</div>
            matches = re.finditer(r'<img src="([^"]+)"[^>]*class="product-image"[^>]*>.*?<h3>([^<]+)</h3>.*?<div class="product-price">EGP([^<]+)</div>', content, re.DOTALL)
            for m in matches:
                img = m.group(1).strip()
                title = m.group(2).strip()
                price = m.group(3).strip()
                prods.append({"title": title, "price": price, "img": img})

print(json.dumps(prods, ensure_ascii=False))
