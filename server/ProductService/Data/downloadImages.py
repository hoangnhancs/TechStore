# import os
# import json
# import requests
# from urllib.parse import urlparse
# import cloudinary
# import cloudinary.uploader

# # Cấu hình Cloudinary (thay bằng thông tin thực tế của bạn)
# cloudinary.config(
#     cloud_name='dukhvtyr7',
#     api_key='297889177929435',
#     api_secret='mKZ4IFpFMFjABpLBpwONgkaNHyY'
# )
# saved_folder = 'TechStore'

# # Đường dẫn file json và thư mục lưu ảnh
# category = 'camera'
# json_path = os.path.join('Jsons', f'{category}_final_data.json')
# with open(json_path, 'r', encoding='utf-8') as f:
#     data = json.load(f)

# item = data[0]
# productname = urlparse(item['url']).path.strip('/').split('/')[-1]

# # Tạo thư mục lưu ảnh
# img_dir = os.path.join('Images', category, productname)
# os.makedirs(img_dir, exist_ok=True)

# def download_image(url, save_dir):
#     filename = os.path.basename(urlparse(url).path)
#     save_path = os.path.join(save_dir, filename)
#     if not os.path.exists(save_path):
#         resp = requests.get(url, timeout=20)
#         if resp.status_code == 200:
#             with open(save_path, 'wb') as f:
#                 f.write(resp.content)
#         else:
#             print(f'Không tải được {url}')
#             return None
#     return save_path

# def upload_cloudinary(local_path):
#     try:
#         res = cloudinary.uploader.upload(local_path, folder=saved_folder)
#         return res['public_id'], res['secure_url']
#     except Exception as e:
#         print(f'Lỗi upload {local_path}: {e}')
#         return None, None

import os
import json
import requests
from urllib.parse import urlparse
import cloudinary
import cloudinary.uploader
from tqdm import tqdm
from datetime import datetime

# Cấu hình Cloudinary (thay bằng thông tin thực tế của bạn)
cloudinary.config(
    cloud_name='dukhvtyr7',
    api_key='297889177929435',
    api_secret='mKZ4IFpFMFjABpLBpwONgkaNHyY'
)
saved_folder = 'TechStore'

json_dir = os.path.join('Jsons')
image_root = 'Images'
failed_log_file = 'failed_operations.txt'

# Khởi tạo file log
with open(failed_log_file, 'w', encoding='utf-8') as f:
    f.write(f'=== LOG CÁC LỖI DOWNLOAD VÀ UPLOAD ===\n')
    f.write(f'Thời gian bắt đầu: {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}\n\n')

# json_files = [f for f in os.listdir(json_dir) if f.endswith('_final_data.json')]
json_files = [f for f in os.listdir(json_dir) if f.endswith('atch_final_data.json')]

def log_failure(message):
    """Ghi log lỗi vào file"""
    with open(failed_log_file, 'a', encoding='utf-8') as f:
        f.write(f'[{datetime.now().strftime("%Y-%m-%d %H:%M:%S")}] {message}\n')

def download_image(url, save_dir, product_name=''):
    filename = os.path.basename(urlparse(url).path)
    save_path = os.path.join(save_dir, filename)
    if not os.path.exists(save_path):
        try:
            resp = requests.get(url, timeout=20)
            if resp.status_code == 200:
                with open(save_path, 'wb') as f:
                    f.write(resp.content)
            else:
                error_msg = f'DOWNLOAD FAILED - Product: {product_name} | URL: {url} | Status: {resp.status_code}'
                print(f'Không tải được {url}')
                log_failure(error_msg)
                return None
        except Exception as e:
            error_msg = f'DOWNLOAD ERROR - Product: {product_name} | URL: {url} | Error: {str(e)}'
            print(f'Lỗi tải {url}: {e}')
            log_failure(error_msg)
            return None
    return save_path

def upload_cloudinary(local_path, folder, product_name=''):
    try:
        res = cloudinary.uploader.upload(local_path, folder=folder)
        return res['public_id'], res['secure_url']
    except Exception as e:
        error_msg = f'UPLOAD ERROR - Product: {product_name} | File: {local_path} | Folder: {folder} | Error: {str(e)}'
        print(f'Lỗi upload {local_path}: {e}')
        log_failure(error_msg)
        return None, None

for json_file in json_files:
    category = json_file.split('_final_data.json')[0]
    json_path = os.path.join(json_dir, json_file)
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    new_data = []
    print(f'Đang xử lý category: {category}...')
    for item in tqdm(data, desc=category, ncols=80, dynamic_ncols=True, leave=False):
        productname = urlparse(item['url']).path.strip('/').split('/')[-1]
        img_dir = os.path.join(image_root, category, productname)
        os.makedirs(img_dir, exist_ok=True)
        # Ảnh chính
        main_img_path = download_image(item['image_url'], img_dir, productname)
        main_publicid, main_url = upload_cloudinary(main_img_path, f'{saved_folder}/{category}/{productname}', productname) if main_img_path else (None, None)
        # Ảnh chi tiết
        imgs_result = []
        for img_url in item.get('imgs', []):
            img_path = download_image(img_url, img_dir, productname)
            publicid, url = upload_cloudinary(img_path, f'{saved_folder}/{category}/{productname}', productname) if img_path else (None, None)
            imgs_result.append({'url': url, 'publicid': publicid})
        # Tạo item mới
        new_item = dict(item)
        new_item['image_url'] = main_url
        new_item['image_publicid'] = main_publicid
        new_item['imgs'] = imgs_result
        new_data.append(new_item)
    # Lưu file json mới
    out_path = os.path.join(json_dir, f'{category}.json')
    with open(out_path, 'w', encoding='utf-8') as f:
        json.dump(new_data, f, ensure_ascii=False, indent=2)
    print(f'Đã xử lý xong {category}, tổng: {len(data)} sản phẩm. File json mới: {out_path}')

# Ghi kết thúc vào log file
with open(failed_log_file, 'a', encoding='utf-8') as f:
    f.write(f'\n=== KẾT THÚC ===\n')
    f.write(f'Thời gian kết thúc: {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}\n')

print('Hoàn thành tất cả category.')
print(f'Kiểm tra file {failed_log_file} để xem các lỗi (nếu có)')
