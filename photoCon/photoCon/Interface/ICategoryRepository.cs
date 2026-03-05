using photoCon.Models;

namespace photoCon.Interface
{
    public interface ICategoryRepository
    {
        Task<ICollection<Category>> GetAllCategories();
        List<Category> GetAllCategoryView();
        bool HasCategory(string categoryName);
        int GetCategoryId(string categoryName);
    }
}
