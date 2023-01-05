using Microsoft.Extensions.Configuration;

namespace Salix.AspNetCore.TitlePage.Tests
{
    public class ConfigurationValuesLoaderTests
    {
        private readonly IConfiguration _configuration;

        public ConfigurationValuesLoaderTests()
        {
            var testConfiguration = new Dictionary<string, string>
            {
                {"KeyAlone1", "Alone1"},
                {"Nested1:Key1", "NestedValue1"},
                {"Nested1:Key2", "NestedValue2"},
                {"Nested2:Key0", "NestedKeyValue0"},
                {"Nested2:KeyVal1:Val1", "NestedKeyValue1"},
                {"Nested2:KeyVal1:Val2", "NestedKeyValue2"},
                {"KeyAlone2", "Alone2"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfiguration)
                .Build();
        }

        [Fact]
        public void GetConfigurationValues_Null_ReturnsAll()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(null);
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(7);
        }

        [Fact]
        public void GetConfigurationValues_Alone_ReturnsOnlyIt()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(new HashSet<string> { "KeyAlone1" });
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(1);
            testable.First().Value.Should().Be("Alone1");
        }

        [Fact]
        public void GetConfigurationValues_Nested1_ReturnsAllSubkeys()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(new HashSet<string> { "Nested1" });
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(2);
        }

        [Fact]
        public void GetConfigurationValues_Nested2_ReturnsAllSubkeys()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(new HashSet<string> { "Nested2" });
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(3);
        }

        [Fact]
        public void GetConfigurationValues_Nested_ReturnsBothNested()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(new HashSet<string> { "Nested" });
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(5);
        }

        [Fact]
        public void GetConfigurationValues_Subkey_ReturnsOnlyIt()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(new HashSet<string> { "Nested2/Key0" });
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(1);
        }

        [Fact]
        public void GetConfigurationValues_Subkey_ReturnsAllChildren()
        {
            var sut = new ConfigurationValuesLoader(_configuration);
            Dictionary<string, string> testable = sut.GetConfigurationValues(new HashSet<string> { "Nested2/KeyVal1" });
            testable.Should().NotBeEmpty();
            testable.Should().HaveCount(2);
        }
    }
}
