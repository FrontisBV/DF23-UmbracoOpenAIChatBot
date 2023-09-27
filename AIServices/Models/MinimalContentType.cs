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

        public class PropertyType
        {
            public string Name { get; set; }
            public string Alias { get; set; }
            public string Description { get; set; }
            public string PropertyEditorAlias { get; set; }
        }
    }
}
