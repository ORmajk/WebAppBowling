using WebAppBowling.Models;

namespace WebAppBowling.Helpers
{
    // поиск картинок в папке wwwroot
    // если в базе записано cola.svg, а в папке лежит cola.jpg - найдёт cola.jpg
    public static class ImageHelper
    {
        public const string NoPhoto = "/images/products/no-photo.svg";

        private static readonly string[] extensions = { ".jpg", ".jpeg", ".png", ".webp", ".svg" };

        // путь к картинке товара (или заглушка, если картинки нет)
        public static string Product(IWebHostEnvironment env, string? imageUrl)
        {
            return Find(env, imageUrl) ?? NoPhoto;
        }

        // путь к фото дорожки: сначала фото из базы (поле Photo),
        // потом шаблон по категории из папки /images/lanes/
        public static string Lane(IWebHostEnvironment env, Lane lane)
        {
            return Find(env, lane.Photo) ?? LaneType(env, lane.LaneType?.Name);
        }

        // фото для категории дорожки: сначала ваши фото (Norm_lanes, VIP_lane, Tiny_lane),
        // потом нарисованные шаблоны (lane-standard, lane-vip, lane-kids)
        public static string LaneType(IWebHostEnvironment env, string? typeName)
        {
            string[] names = { "Norm_lanes", "lane-standard" };
            if (typeName == "VIP") names = new[] { "VIP_lane", "lane-vip" };
            if (typeName == "Детская") names = new[] { "Tiny_lane", "lane-kids" };

            foreach (string name in names)
            {
                string? found = Find(env, "/images/lanes/" + name);
                if (found != null) return found;
            }
            return Find(env, "/images/lanes/Norm_lanes") ?? Find(env, "/images/lanes/lane-standard") ?? NoPhoto;
        }

        // ищет файл; если такого нет - пробует то же имя с другими расширениями,
        // а потом ищет файл с таким именем в папках images/lanes и images/products
        public static string? Find(IWebHostEnvironment env, string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // ссылки из интернета не проверяем
            if (path.StartsWith("http://") || path.StartsWith("https://"))
                return path;

            // "\images\VIP_lane.jpg" -> "/images/VIP_lane.jpg"
            string webPath = "/" + path.Trim().Replace('\\', '/').TrimStart('~', '/');

            string? found = FindWithExtensions(env, webPath);
            if (found != null)
                return found;

            // файл лежит в другой папке? ищем по имени
            string fileName = Path.GetFileName(webPath);
            foreach (string folder in new[] { "/images/lanes/", "/images/products/", "/images/" })
            {
                found = FindWithExtensions(env, folder + fileName);
                if (found != null)
                    return found;
            }
            return null;
        }

        // проверяет путь как есть и с расширениями .jpg .jpeg .png .webp .svg
        private static string? FindWithExtensions(IWebHostEnvironment env, string webPath)
        {
            if (Exists(env, webPath))
                return webPath;

            string ext = Path.GetExtension(webPath);
            string withoutExt = ext.Length > 0 ? webPath.Substring(0, webPath.Length - ext.Length) : webPath;
            foreach (string e in extensions)
            {
                if (Exists(env, withoutExt + e))
                    return withoutExt + e;
            }
            return null;
        }

        private static bool Exists(IWebHostEnvironment env, string webPath)
        {
            string fullPath = Path.Combine(env.WebRootPath, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(fullPath);
        }
    }
}
