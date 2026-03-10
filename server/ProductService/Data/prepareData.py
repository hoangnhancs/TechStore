import json
import os

# Đường dẫn thư mục chứa các file json
json_folder = os.path.dirname(__file__) + '/Jsons'

# Hàm xử lý từng file
def process_file(file_path):

    filename = os.path.basename(file_path)
    # Kiểm tra tên file đúng định dạng *_attributes_data_full.json
    if not filename.endswith('_final_data.json'):
        print(f'Skipping file {filename}, does not match pattern.')
        return

    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    print(f'Processing file: {filename}, number of items: {len(data)}')
    for item in data:
        # Nếu description là list, nối lại thành string
        if 'descriptions' in item and isinstance(item['descriptions'], list):
            # print(f'Processing item ID {item.get("id", "unknown")} in file {filename}')
            item['descriptions'] = '. '.join([str(x).strip() for x in item['descriptions'] if x]).replace('..', '.')
    with open(file_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

# Lặp qua tất cả các file *_attributes_data_full.json trong thư mục
for filename in os.listdir(json_folder):
    if filename.endswith('_final_data.json'):
        process_file(os.path.join(json_folder, filename))

print('Done!')
