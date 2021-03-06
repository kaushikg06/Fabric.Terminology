﻿namespace Fabric.Terminology.UnitTests.Caching
{
    using System;
    using Fabric.Terminology.SqlServer.Caching;
    using Fabric.Terminology.TestsBase.Fixtures;
    using Fabric.Terminology.TestsBase.Mocks;

    using FluentAssertions;
    using Xunit;

    public class MemoryCacheProviderTests : IClassFixture<AppConfigurationFixture>
    {
        private readonly AppConfigurationFixture fixture;

        public MemoryCacheProviderTests(AppConfigurationFixture classFixture)
        {
            this.fixture = classFixture;
        }

        [Fact]
        public void GetItemReturnsNullWhenNotFound()
        {
            // Arrange
            var cache = new MemoryCacheProvider(this.fixture.AppConfiguration.TerminologySqlSettings);
            const string Key = "key";

            // Act
            var val = cache.GetItem(Key);

            // Assert
            Assert.Null(val);
        }

        [Fact]
        public void GetItemCanAddToCache()
        {
            // Arrange
            var cache = new MemoryCacheProvider(this.fixture.AppConfiguration.TerminologySqlSettings);
            const string Key = "key";
            var dt = DateTime.Now;
            var obj = new TestObject {Text = "Test", Stamp = dt };

            // Act
            var tmp = cache.GetItem(Key, () => obj);
            var cached = cache.GetItem(Key) as TestObject;

            // Assert
            Assert.NotNull(cached);
            Assert.Equal(dt, cached.Stamp);
        }

        [Fact]
        public void ClearItemClearsSingleItem()
        {
            // Arrange
            var cache = new MemoryCacheProvider(this.fixture.AppConfiguration.TerminologySqlSettings);
            const string Key1 = "key1";
            const string Key2 = "key2";
            const string Key3 = "key3";
            var obj1 = new TestObject { Text = "Test1", Stamp = DateTime.Now };
            var obj2 = new TestObject { Text = "Test2", Stamp = DateTime.Now };
            var obj3 = new TestObject { Text = "Test3", Stamp = DateTime.Now };

            cache.GetItem(Key1, () => obj1);
            cache.GetItem(Key2, () => obj2);
            cache.GetItem(Key3, () => obj3);

            // Act
            cache.GetItem(Key2).Should().NotBeNull();
            cache.ClearItem(Key2);

            // Assert
            cache.GetItem(Key2).Should().BeNull();
            cache.GetItem(Key1).Should().NotBeNull();
            cache.GetItem(Key3).Should().NotBeNull();
        }

        [Fact]
        public void ClearAllClearsAllCache()
        {
            // Arrange
            var cache = new MemoryCacheProvider(this.fixture.AppConfiguration.TerminologySqlSettings);
            const string Key1 = "key1";
            const string Key2 = "key2";
            const string Key3 = "key3";
            var obj1 = new TestObject { Text = "Test1", Stamp = DateTime.Now };
            var obj2 = new TestObject { Text = "Test2", Stamp = DateTime.Now };
            var obj3 = new TestObject { Text = "Test3", Stamp = DateTime.Now };

            cache.GetItem(Key1, () => obj1);
            cache.GetItem(Key2, () => obj2);
            cache.GetItem(Key3, () => obj3);

            cache.GetItem(Key1).Should().NotBeNull();
            cache.GetItem(Key2).Should().NotBeNull();
            cache.GetItem(Key3).Should().NotBeNull();

            var instanceKey = cache.InstanceKey;

            // Act
            cache.ClearAll();

            // Assert
            cache.GetItem(Key1).Should().BeNull();
            cache.GetItem(Key2).Should().BeNull();
            cache.GetItem(Key3).Should().BeNull();
            instanceKey.Should().NotBe(cache.InstanceKey);
        }

        [Fact]
        public void GetItemAddsItemByFunction()
        {
            // Arrange
            var cache = new MemoryCacheProvider(this.fixture.AppConfiguration.TerminologySqlSettings);
            const string Key = "key";
            cache.GetItem(Key).Should().BeNull();

            // Act
            cache.GetItem(Key, () => new TestObject { Text = "Test string", Stamp = DateTime.Now });

            // Assert
            cache.GetItem(Key).Should().NotBeNull();
        }
    }
}
