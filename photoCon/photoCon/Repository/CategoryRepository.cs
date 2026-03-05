using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Interface;
using photoCon.Models;

namespace photoCon.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;
        public CategoryRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public async Task<ICollection<Category>> GetAllCategories()
        {
           return await _tallyProgramContext.Categories.ToListAsync();
        }

        public List<Category> GetAllCategoryView()
        {
            List<Category> categories = new List<Category>();

            // Use ToList() to explicitly convert IQueryable<RegionView> to List<RegionView>
            categories = (from r in _tallyProgramContext.Categories
                       select new Category
                       {
                           CategoryId = r.CategoryId,
                           CategoryName = r.CategoryName,
                       }).ToList();
            return categories;
        }

        public int GetCategoryId(string categoryName)
        {

            var categoryId = (from a in _tallyProgramContext.Categories
                            where a.CategoryName == categoryName
                            select a.CategoryId).FirstOrDefault();

            return categoryId;
            
        }

        public bool HasCategory(string categoryName)
        {
            return _tallyProgramContext.Categories.Any(c => c.CategoryName == categoryName);
        }
    }
}
