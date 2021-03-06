﻿namespace Fabric.Terminology.TestsBase.Fixtures
{
    using System;
    using System.IO;

    using AutoMapper;

    using Fabric.Terminology.API.Configuration;
    using Fabric.Terminology.API.Models;
    using Fabric.Terminology.Domain;
    using Fabric.Terminology.Domain.Models;

    using Microsoft.Extensions.Configuration;

    public class AppConfigurationFixture : TestFixtureBase
    {
        public AppConfigurationFixture()
        {
            Mapper.Initialize(
                cfg =>
                    {
                        cfg.CreateMap<IValueSet, ValueSetApiModel>()
                            .ForMember(
                                dest => dest.Identifier,
                                opt => opt.MapFrom(
                                    src => src.ValueSetId.IsNullOrWhiteSpace()
                                               ? Guid.NewGuid().ToString()
                                               : src.ValueSetId));

                        cfg.CreateMap<IValueSetCode, ValueSetCodeApiModel>()
                            .ForMember(dest => dest.CodeSystemCode, opt => opt.MapFrom(src => src.CodeSystem.Code));
                    });

            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(TestHelper.GetAppConfigFile().FullName);

            this.Configuration = builder.Build();

            this.AppConfiguration = new AppConfiguration();
            this.Configuration.Bind(this.AppConfiguration);
        }

        public IAppConfiguration AppConfiguration { get; }

        protected IConfigurationRoot Configuration { get; }
    }
}