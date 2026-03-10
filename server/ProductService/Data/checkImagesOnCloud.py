import os
import json

json_dir = 'Jsons'
output_file = 'cloudinary_images_report.txt'

# Chỉ lấy các file không chứa dấu _ (camera.json, phone.json, tv.json...)
json_files = [f for f in os.listdir(json_dir) if f.endswith('.json') and '_' not in f]

cloudinary_images = []
cellphones_images = []
string_imgs = []
total_products = 0
total_images = 0

print('Đang kiểm tra các file JSON...\n')
print(f'Files được check: {json_files}\n')

for json_file in json_files:
    json_path = os.path.join(json_dir, json_file)
    category = json_file.replace('.json', '')
    
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    print(f'📁 Checking {category}: {len(data)} products')
    
    for item in data:
        total_products += 1
        product_name = item.get('name', 'Unknown')
        
        # Kiểm tra ảnh chính
        main_url = item.get('image_url', '')
        if main_url:
            if 'cloudinary' in main_url.lower():
                cloudinary_images.append({
                    'category': category,
                    'product': product_name,
                    'type': 'main_image',
                    'url': main_url
                })
            elif 'cellphones' in main_url.lower():
                cellphones_images.append({
                    'category': category,
                    'product': product_name,
                    'type': 'main_image',
                    'url': main_url
                })
        
        # Kiểm tra ảnh chi tiết
        for img_idx, img in enumerate(item.get('imgs', [])):
            total_images += 1
            
            # Kiểm tra nếu img là string
            if isinstance(img, str):
                string_imgs.append({
                    'category': category,
                    'product': product_name,
                    'index': img_idx,
                    'value': img
                })
                img_url = img
            else:
                img_url = img.get('url', '')
            
            if img_url:
                if 'cloudinary' in img_url.lower():
                    cloudinary_images.append({
                        'category': category,
                        'product': product_name,
                        'type': f'imgs[{img_idx}]',
                        'url': img_url
                    })
                elif 'cellphones' in img_url.lower():
                    cellphones_images.append({
                        'category': category,
                        'product': product_name,
                        'type': f'imgs[{img_idx}]',
                        'url': img_url
                    })

# Thống kê theo category
cloudinary_by_category = {}
cellphones_by_category = {}

for item in cloudinary_images:
    cat = item['category']
    cloudinary_by_category[cat] = cloudinary_by_category.get(cat, 0) + 1

for item in cellphones_images:
    cat = item['category']
    cellphones_by_category[cat] = cellphones_by_category.get(cat, 0) + 1

# In kết quả ra console
print('\n' + '=' * 80)
print('KẾT QUẢ KIỂM TRA IMAGES')
print('=' * 80)
print(f'Tổng số sản phẩm: {total_products}')
print(f'Tổng số ảnh (bao gồm main + imgs): {total_images + total_products}')
print(f'\n✅ Ảnh trên Cloudinary: {len(cloudinary_images)}')
print(f'❌ Ảnh từ Cellphones: {len(cellphones_images)}')
print(f'⚠️  Imgs là string: {len(string_imgs)}')

print('\n📊 THỐNG KÊ THEO CATEGORY:')
print('-' * 80)
all_categories = set(list(cloudinary_by_category.keys()) + list(cellphones_by_category.keys()))
for cat in sorted(all_categories):
    cloudinary_count = cloudinary_by_category.get(cat, 0)
    cellphones_count = cellphones_by_category.get(cat, 0)
    total = cloudinary_count + cellphones_count
    percentage = (cloudinary_count / total * 100) if total > 0 else 0
    print(f'{cat:15} | Cloudinary: {cloudinary_count:4} | Cellphones: {cellphones_count:4} | {percentage:.1f}% uploaded')

# Ghi kết quả ra file
with open(output_file, 'w', encoding='utf-8') as f:
    f.write('=' * 80 + '\n')
    f.write('KIỂM TRA IMAGES - CLOUDINARY vs CELLPHONES\n')
    f.write('=' * 80 + '\n\n')
    f.write(f'Tổng số sản phẩm: {total_products}\n')
    f.write(f'Tổng số ảnh: {total_images + total_products}\n')
    f.write(f'Ảnh trên Cloudinary: {len(cloudinary_images)}\n')
    f.write(f'Ảnh từ Cellphones: {len(cellphones_images)}\n')
    f.write(f'Imgs là string: {len(string_imgs)}\n\n')
    
    f.write('THỐNG KÊ THEO CATEGORY:\n')
    f.write('-' * 80 + '\n')
    for cat in sorted(all_categories):
        cloudinary_count = cloudinary_by_category.get(cat, 0)
        cellphones_count = cellphones_by_category.get(cat, 0)
        total = cloudinary_count + cellphones_count
        percentage = (cloudinary_count / total * 100) if total > 0 else 0
        f.write(f'{cat:15} | Cloudinary: {cloudinary_count:4} | Cellphones: {cellphones_count:4} | {percentage:.1f}% uploaded\n')
    
    if cellphones_images:
        f.write('\n' + '=' * 80 + '\n')
        f.write('CHI TIẾT ẢNH TỪ CELLPHONES (CẦN UPLOAD)\n')
        f.write('=' * 80 + '\n\n')
        
        current_category = None
        for item in cellphones_images:
            if current_category != item['category']:
                current_category = item['category']
                f.write(f'\n📁 Category: {current_category}\n')
                f.write('-' * 80 + '\n')
            
            f.write(f'  Product: {item["product"]}\n')
            f.write(f'  Type: {item["type"]}\n')
            f.write(f'  URL: {item["url"]}\n\n')
    
    if string_imgs:
        f.write('\n' + '=' * 80 + '\n')
        f.write('CÁC IMGS LÀ STRING (CẦN FIX)\n')
        f.write('=' * 80 + '\n\n')
        for item in string_imgs:
            f.write(f'  Category: {item["category"]}\n')
            f.write(f'  Product: {item["product"]}\n')
            f.write(f'  Index: {item["index"]}\n')
            f.write(f'  Value: {item["value"]}\n\n')

print(f'\n✅ Đã lưu báo cáo chi tiết vào: {output_file}')