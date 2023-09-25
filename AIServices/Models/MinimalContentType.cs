using static AIServices.Models.MinimalContentItem;

namespace AIServices.Models
{
    public class MinimalContentType
    {
        public int Id { get; set; }

        public int ContentTypeId { get; set; }

        public int Level { get; set; }
        public int ParentId { get; set; }

        public string Name { get; set; }

        public List<PropertyType> PropertyTypes { get; set; }
    }
}
