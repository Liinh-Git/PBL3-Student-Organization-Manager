# EventCategories Services

## ICategoryService / CategoryService
**Methods**:
- `Task<List<CategoryDto>> ListCategoriesAsync(Guid milestoneId, Guid userId)`
- `Task<CategoryDto> CreateCategoryAsync(Guid milestoneId, CreateCategoryRequest request, Guid userId)`
- `Task<CategoryDto> GetCategoryAsync(Guid categoryId, Guid userId)`
- `Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, Guid userId)`
- `Task DeleteCategoryAsync(Guid categoryId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
