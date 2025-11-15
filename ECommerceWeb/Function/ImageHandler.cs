namespace ECommerceWeb.Function
{
    public class ImageHandler
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageHandler(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string SaveImage(IFormFile file, string folderPath, string filePath)
        {
            // Địa chỉ của thư mục wwwroot
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            // Tên file mới = Id độc nhất + đuôi file (.jpg, .png, .jpeg, ...)
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            // Địa chỉ của thư mục sẽ được copy ảnh sang
            string fullFolderPath = Path.Combine(wwwRootPath, folderPath);

            if (!string.IsNullOrEmpty(filePath))
            {
                // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                var oldImagePath = Path.Combine(wwwRootPath, filePath.TrimStart('\\'));

                // Nếu có rồi thì xóa luôn cái cũ
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Copy ảnh vừa chọn vào thư mục
            using (var fileStream = new FileStream(Path.Combine(fullFolderPath, fileName), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            // Gán ImageUrl = đường dẫn của ảnh vừa copy
            return "\\" + folderPath + fileName;
        }

        public void DeleteImage(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('\\'));

                // Nếu có rồi thì xóa luôn cái cũ
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
        }
    }
}
