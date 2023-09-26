using AIServices.Models;
using AIServices.Models.ContentItems;
using AutoMapper;

namespace AIServices.Mappings
{

    public class UmbacoContentMappings : Profile
    {
        public UmbacoContentMappings()
        {
            CreateMap<Umbraco.Cms.Core.Models.Content, MinimalContentItem>();
            CreateMap<Umbraco.Cms.Core.Models.Property, MinimalContentItem.Property>();
            CreateMap<Umbraco.Cms.Core.Models.PropertyType, MinimalContentItem.PropertyType>();
            CreateMap<Umbraco.Cms.Core.Models.PropertyGroup, MinimalContentItem.PropertyGroup>();
            CreateMap<Umbraco.Cms.Core.Models.IPropertyValue, MinimalContentItem.PropertyValue>();

            CreateMap<Umbraco.Cms.Core.Models.ContentType, MinimalContentType>();
        }
    }
}
