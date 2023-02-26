using AutoMapper;
using Blogfolio_CORE.Areas.Admin.ViewModels;
using Blogfolio_CORE.Models;
using System;

namespace Blogfolio_CORE
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<CategoryEditModel, Category>()
                .ForMember(q => q.CategoryId, d => d.MapFrom(x => x.CategoryId ?? Guid.Empty));

            CreateMap<Category, CategoryEditModel>();

            CreateMap<PostEditModel, Post>()
                .ForMember(q => q.PostId, d => d.MapFrom(x => x.PostId ?? Guid.Empty));

            CreateMap<Post, PostEditModel>();

            CreateMap<ProjectEditModel, Project>()
                .ForMember(q => q.ProjectId, d => d.MapFrom(x => x.ProjectId ?? Guid.Empty));

            CreateMap<Project, ProjectEditModel>();
        }
    }
}
