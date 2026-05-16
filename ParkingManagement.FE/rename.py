import os

replacements = {
    'ViewData["Title"] = "Tổng quan Admin";': 'ViewData["Title"] = "Tổng quan";',
    'ViewData["Title"] = "Tổng quan Khách hàng";': 'ViewData["Title"] = "Tổng quan";',
    'ViewData["Title"] = "Quản lý vé";': 'ViewData["Title"] = "Quản lý vé lượt";',
    'ViewData["Title"] = "Quản lý vé xe";': 'ViewData["Title"] = "Quản lý vé lượt";',
    'ViewData["Title"] = "Vé tháng";': 'ViewData["Title"] = "Quản lý vé tháng";',
    'ViewData["Title"] = "Quản lý đơn đặt chỗ";': 'ViewData["Title"] = "Quản lý đặt chỗ";',
    '<i class="fa-solid fa-chart-line"></i> Doanh thu': '<i class="fa-solid fa-chart-line"></i> Báo cáo doanh thu',
    '<i class="fa-solid fa-user-group"></i> Khách hàng': '<i class="fa-solid fa-user-group"></i> Thống kê khách hàng',
    '<i class="fa-solid fa-chart-column"></i> Tổng hợp': '<i class="fa-solid fa-chart-column"></i> Báo cáo tổng hợp',
    '<i class="fa-solid fa-ticket"></i> Quản lý vé': '<i class="fa-solid fa-ticket"></i> Quản lý vé lượt',
    '<i class="fa-solid fa-calendar-check"></i> Vé tháng': '<i class="fa-solid fa-calendar-check"></i> Quản lý vé tháng',
    '<i class="fa-solid fa-square-parking"></i> Chỗ đỗ xe': '<i class="fa-solid fa-square-parking"></i> Quản lý chỗ đỗ',
    '<i class="fa-solid fa-clipboard-user"></i> Cá nhân': '<i class="fa-solid fa-clipboard-user"></i> Báo cáo cá nhân',
    '<i class="fa-solid fa-calendar-check"></i> Đơn đặt chỗ': '<i class="fa-solid fa-calendar-check"></i> Quản lý đặt chỗ',
    '<i class="fa-solid fa-ticket"></i> Vé xe': '<i class="fa-solid fa-ticket"></i> Quản lý vé lượt',
    '<i class="fa-solid fa-credit-card"></i> Vé tháng': '<i class="fa-solid fa-credit-card"></i> Quản lý vé tháng',
}

def replace_in_files(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.cshtml') or file.endswith('.cs'):
                path = os.path.join(root, file)
                try:
                    with open(path, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    new_content = content
                    for old, new in replacements.items():
                        new_content = new_content.replace(old, new)
                        
                    if new_content != content:
                        with open(path, 'w', encoding='utf-8') as f:
                            f.write(new_content)
                        print(f'Updated {path}')
                except Exception as e:
                    print(f'Error reading {path}: {e}')

replace_in_files(r'd:\pbl3\PBL3_ParkingManagementSystem\ParkingManagement.FE\Pages')
