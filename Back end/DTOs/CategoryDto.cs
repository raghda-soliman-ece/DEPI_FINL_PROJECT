namespace Jumia.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PictureUrl { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
