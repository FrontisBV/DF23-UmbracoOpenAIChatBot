namespace AIServices.Models
{
    public class MinimalContentItem
    {
        public int Id { get; set; }

        public int ContentTypeId { get; set; }

        public int Level { get; set; }
        public int ParentId { get; set; }

        public string Name { get; set; }
        public List<Property> Properties { get; set; }

        public class Property
        {
            public string Alias { get; set; }
            public int PropertyTypeId { get; set; }
            public PropertyType PropertyType { get; set; }

            public List<PropertyValue> Values { get; set; }
        }

        public class PropertyType
        {
            public string Name { get; set; }
            public string Alias { get; set; }

            public string Description { get; set; }

            public string PropertyEditorAlias { get; set; }
        }

        public class PropertyValue
        {
            public string Culture { get; set; }
            public string PublishedValue { get; set; }
        }
    }
}
