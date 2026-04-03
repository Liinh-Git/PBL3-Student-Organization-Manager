namespace Org.Backend.Features.EventCategories;

internal static class EventCategoriesFeatureTodos
{
    // TODO(BE-DAY1): POST /api/milestones/{milestoneId}/categories
    //  - Validate milestone exists and milestoneId matches body.MilestoneId.
    //  - Validate ParentCategoryId belongs to same milestone.
    //  - Return 201 with created EventCategoryDto.
    //
    // TODO(BE-DAY1): GET /api/milestones/{milestoneId}/categories
    //  - Return flat list with ParentCategoryId for FE tree builder.
    //  - Return 200 with { items: [...] }.
    //
    // TODO(BE-DAY1): Add mapping layer for tree projection if FE requests nested response later.
}
