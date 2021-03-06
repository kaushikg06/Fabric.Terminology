﻿namespace Fabric.Terminology.UnitTests.AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Fabric.Terminology.API.Models;
    using Fabric.Terminology.Domain.Models;
    using Fabric.Terminology.TestsBase;
    using Fabric.Terminology.TestsBase.Fixtures;

    using FluentAssertions;

    using global::AutoMapper;

    using JetBrains.Annotations;

    using Xunit;

    public class AutoMapperTests : IClassFixture<AppConfigurationFixture>
    {
        private readonly AppConfigurationFixture fixture;

        public AutoMapperTests(AppConfigurationFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void CanMapValueSetToValueSetItem()
        {
            // Arrange
            var valueSet = DataHelper.SingleValueSet("1", 3);

            // Act
            var mapped = Mapper.Map<IValueSet, ValueSetApiModel>(valueSet);

            // Assert
            mapped.Should().NotBeNull();
            mapped.ShouldBeEquivalentTo(valueSet, o => o.Excluding(p => p.Identifier).Excluding(p => p.ValueSetCodes));
            mapped.Identifier.ShouldBeEquivalentTo(valueSet.ValueSetId);

            var codes = valueSet.ValueSetCodes.ToArray();
            var mappedCodes = mapped.ValueSetCodes.ToArray();
            for (var i = 0; i < codes.Length; i++)
            {
                var code = codes[i];
                mappedCodes[i].ShouldBeEquivalentTo(code, o => o.ExcludingMissingMembers());
                mappedCodes[i].CodeSystemCode.Should().Be(code.CodeSystem.Code);
            }
        }
    }
}