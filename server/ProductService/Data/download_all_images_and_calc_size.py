import os
import glob
import json
import requests
from urllib.parse import urlparse

# Tìm tất cả file *_final_data.json
json_dir = os.path.join(os.path.dirname(__file__), 'Jsons')
print("Tìm trong thư mục:", json_dir)
json_files = glob.glob(os.path.join(json_dir, '*_final_data.json'))

def get_all_image_urls(json_files):
    urls = set()
    for file in json_files:
        with open(file, 'r', encoding='utf-8') as f:
            try:
                data = json.load(f)
                for item in data:
                    # Lấy các trường có thể chứa url ảnh
                    if 'image_url' in item:
                        urls.add(item['image_url'])
                    if 'imgs' in item and isinstance(item['imgs'], list):
                        for img_url in item['imgs']:
                            urls.add(img_url)
            except Exception as e:
                print(f"Lỗi đọc {file}: {e}")
    return list(urls)

def download_file(url, dest_folder):
    try:
        os.makedirs(dest_folder, exist_ok=True)
        filename = os.path.basename(urlparse(url).path)
        dest_path = os.path.join(dest_folder, filename)
        if os.path.exists(dest_path):
            return os.path.getsize(dest_path)
        resp = requests.get(url, timeout=20)
        if resp.status_code == 200:
            with open(dest_path, 'wb') as f:
                f.write(resp.content)
            return len(resp.content)
        else:
            print(f"Không tải được {url} (status {resp.status_code})")
            return 0
    except Exception as e:
        print(f"Lỗi tải {url}: {e}")
        return 0

def main():
    for path in json_files:
        print("Found json file:", path)
    urls = get_all_image_urls(json_files)
    print(f"Tổng số url: {len(urls)}")
    total_size = 0
    for url in urls:
        size = download_file(url, 'downloaded_images')
        total_size += size
        print(f"Đã tải: {url} ({size/1024:.1f} KB)")
    print(f"Tổng dung lượng đã tải: {total_size/1024/1024:.2f} MB")

if __name__ == '__main__':
    main()
