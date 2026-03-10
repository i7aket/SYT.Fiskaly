using AwesomeAssertions;
using SYT.Fiskaly.Authentication;
using SYT.Fiskaly.Authentication.Credentials;
using SYT.Fiskaly.Authentication.ValueObjects;

namespace SYT.Fiskaly.UnitTests.Authentication;

public class FiskalyCredentialScopeFactoryTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Use_SetsCurrentCredentials_AndClearsAfterDispose()
    {
        FiskalyCredentialScopeFactory sut = new();
        ApiKeyCredentials credentials = CreateCredentials("test_scope_key_a");

        using (sut.Use(credentials))
        {
            sut.Current.Should().BeSameAs(credentials);
        }

        sut.Current.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Use_WithNestedScopes_RestoresPreviousCredentials()
    {
        FiskalyCredentialScopeFactory sut = new();
        ApiKeyCredentials outerCredentials = CreateCredentials("test_scope_key_outer");
        ApiKeyCredentials innerCredentials = CreateCredentials("test_scope_key_inner");

        using (sut.Use(outerCredentials))
        {
            sut.Current.Should().BeSameAs(outerCredentials);

            using (sut.Use(innerCredentials))
            {
                sut.Current.Should().BeSameAs(innerCredentials);
            }

            sut.Current.Should().BeSameAs(outerCredentials);
        }

        sut.Current.Should().BeNull();
    }

    private static ApiKeyCredentials CreateCredentials(string apiKey) =>
        new(ApiKey.From(apiKey), ApiSecret.From("1234567890123456789012345678901234567890123"));
}
