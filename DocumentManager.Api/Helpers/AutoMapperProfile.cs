﻿using AutoMapper;
using DocumentManager.Api.Dtos;
using DocumentManager.Api.Models;

namespace DocumentManager.Api.Helpers
{
    /// <summary>
    /// Class for configuring related object mappings
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Document, DocumentDto>();
        }
    }
}
