﻿namespace Fabric.Terminology.API.Configuration
{
    using Fabric.Terminology.SqlServer.Configuration;

    public class AppConfiguration : IAppConfiguration
    {
        public TerminologySqlSettings TerminologySqlSettings { get; set; }

        public ValueSetSettings ValueSetSettings { get; set; }
    }
}