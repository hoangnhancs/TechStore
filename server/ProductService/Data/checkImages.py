import os
import json

json_dir = 'Jsons'
output_file = 'cellphones_urls_check.txt'

# Chỉ lấy các file không chứa dấu _ (camera.json, phone.json, tv.json...)
json_files = [f for f in os.listdir(json_dir) if f.endswith('.json') and '_' not in f]

cellphones_found = []
total_products = 0
total_images = 0
string_imgs_found = []  # Log các imgs là string

print('Đang kiểm tra các file JSON...\n')
print(f'Files được check: {json_files}\n')

for json_file in json_files:
    json_path = os.path.join(json_dir, json_file)
    category = json_file.replace('.json', '')
    
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    print(f'📁 Checking {category}: {len(data)} products')
    
    for idx, item in enumerate(data):
        total_products += 1
        product_name = item.get('name', 'Unknown')
        
        # Kiểm tra ảnh chính
        main_url = item.get('image_url', '')
        if main_url and 'cellphones' in main_url.lower():
            cellphones_found.append({
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
                string_imgs_found.append({
                    'category': category,
                    'product': product_name,
                    'index': img_idx,
                    'value': img
                })
                img_url = img
            else:
                img_url = img.get('url', '')
            
            if img_url and 'cellphones' in img_url.lower():
                cellphones_found.append({
                    'category': category,
                    'product': product_name,
                    'type': f'detail_image_{img_idx + 1}',
                    'url': img_url
                })

# In ra các imgs là string
if string_imgs_found:
    print('\n' + '=' * 80)
    print('⚠️  CÁC IMGS LÀ STRING (KHÔNG PHẢI OBJECT)')
    print('=' * 80)
    for item in string_imgs_found:
        print(f'  Category: {item["category"]}')
        print(f'  Product: {item["product"]}')
        print(f'  Index: {item["index"]}')
        print(f'  Value: {item["value"][:100]}...' if len(item["value"]) > 100 else f'  Value: {item["value"]}')
        print('-' * 40)

# Ghi kết quả ra file
with open(output_file, 'w', encoding='utf-8') as f:
    f.write('=' * 80 + '\n')
    f.write('KIỂM TRA URL CELLPHONES TRONG CÁC FILE JSON\n')
    f.write('=' * 80 + '\n\n')
    f.write(f'Tổng số sản phẩm: {total_products}\n')
    f.write(f'Tổng số ảnh chi tiết: {total_images}\n')
    f.write(f'Số URL cellphones tìm thấy: {len(cellphones_found)}\n')
    f.write(f'Số imgs là string: {len(string_imgs_found)}\n\n')
    
    if string_imgs_found:
        f.write('=' * 80 + '\n')
        f.write('CÁC IMGS LÀ STRING (KHÔNG PHẢI OBJECT)\n')
        f.write('=' * 80 + '\n\n')
        for item in string_imgs_found:
            f.write(f'  Category: {item["category"]}\n')
            f.write(f'  Product: {item["product"]}\n')
            f.write(f'  Index: {item["index"]}\n')
            f.write(f'  Value: {item["value"]}\n\n')
    
    if cellphones_found:
        f.write('=' * 80 + '\n')
        f.write('CHI TIẾT CÁC URL CELLPHONES\n')
        f.write('=' * 80 + '\n\n')
        
        current_category = None
        for item in cellphones_found:
            if current_category != item['category']:
                current_category = item['category']
                f.write(f'\n📁 Category: {current_category}\n')
                f.write('-' * 80 + '\n')
            
            f.write(f'  Sản phẩm: {item["product"]}\n')
            f.write(f'  Loại: {item["type"]}\n')
            f.write(f'  URL: {item["url"]}\n\n')
    else:
        f.write('\n✅ KHÔNG TÌM THẤY URL CELLPHONES NÀO!\n')
        f.write('Tất cả ảnh đã được upload lên Cloudinary.\n')

# In kết quả ra console
print('\n' + '=' * 80)
print('KẾT QUẢ KIỂM TRA')
print('=' * 80)
print(f'Tổng số sản phẩm: {total_products}')
print(f'Tổng số ảnh chi tiết: {total_images}')
print(f'Số URL cellphones tìm thấy: {len(cellphones_found)}')
print(f'Số imgs là string: {len(string_imgs_found)}')

if cellphones_found:
    print('\n⚠️  CẢNH BÁO: Vẫn còn URL từ cellphones.vn!')
    print(f'Chi tiết đã được ghi vào file: {output_file}')
    
    # Hiển thị một số ví dụ
    print('\nMột số ví dụ:')
    for item in cellphones_found[:5]:
        print(f'  - {item["category"]}/{item["product"]} ({item["type"]})')
else:
    print('\n✅ HOÀN HẢO! Không tìm thấy URL cellphones nào.')
    print('Tất cả ảnh đã được upload lên Cloudinary.')

print(f'\nĐã lưu báo cáo chi tiết vào: {output_file}')