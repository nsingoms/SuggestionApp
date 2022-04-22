using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess
{
    public class MongoCategoryData
    {
        private readonly IMemoryCache _cache;
        private readonly IMongoCollection<CategoryModel> _categories;

        public MongoCategoryData(IDbConnection db,IMemoryCache cache)
        {
            _cache = cache;
            _categories = db.CategoryCollection;
        }
    }
}
