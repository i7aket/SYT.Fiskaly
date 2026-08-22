using SYT.Fiskaly.Authentication.ValueObjects;

namespace SYT.Fiskaly.UnitTests.Authentication.ValueObjects;

/// <summary>
/// Tests for ApiSecret value object validation.
/// </summary>
/// <remarks>
/// The rule is deliberately weak, and these tests are what keeps it weak: a fiskaly API secret is minted
/// by fiskaly, so the only things this type may honestly assert about one are that something is there and
/// that it is not absurdly long - the same bounds the sibling ApiKey uses.
/// <para>
/// It used to demand exactly 43 alphanumeric characters, and this file enforced that: a 42-character
/// secret HAD to throw, underscores HAD to be rejected. Then fiskaly issued a managed-organisation secret
/// of 42 characters and every provisioning call failed with a FormatException that blamed the caller for
/// the vendor's own value. The old rule also rejected underscores while the message it threw advertised
/// the format "test_xxx_xxx" - the validation contradicted its own explanation, and the tests agreed with
/// both at once because they only ever asserted the substring "exactly 43 alphanumeric characters".
/// </para>
/// </remarks>
public class ApiSecretTests
{
    #region From Method - Valid Inputs

    [Trait("Category", "Unit")]
    [Fact]
    public void From_ValidApiSecret43Chars_Succeeds()
    {
        // Arrange: Valid 43-char alphanumeric secret
        string validSecret = "abcdefghijklmnopqrstuvwxyz01234567890ABCDEF"; // Exactly 43 chars

        // Act
        ApiSecret apiSecret = ApiSecret.From(validSecret);

        // Assert
        Assert.Equal(validSecret, apiSecret.Value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_ValidApiSecretWithNumbers_Succeeds()
    {
        // Arrange: All numeric characters (43 digits)
        string validSecret = "0123456789012345678901234567890123456789012"; // Exactly 43 chars

        // Act
        ApiSecret apiSecret = ApiSecret.From(validSecret);

        // Assert
        Assert.Equal(validSecret, apiSecret.Value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_ValidApiSecretMixedCase_Succeeds()
    {
        // Arrange: Mixed case alphanumeric (realistic Fiskaly format)
        string validSecret = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"; // Exactly 43 chars

        // Act
        ApiSecret apiSecret = ApiSecret.From(validSecret);

        // Assert
        Assert.Equal(validSecret, apiSecret.Value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_ValidApiSecretTrimsWhitespace_Succeeds()
    {
        // Arrange: Valid secret with surrounding whitespace
        string secretWithWhitespace = "  abcdefghijklmnopqrstuvwxyz01234567890ABCDEF  ";
        string expectedTrimmed = "abcdefghijklmnopqrstuvwxyz01234567890ABCDEF";

        // Act
        ApiSecret apiSecret = ApiSecret.From(secretWithWhitespace);

        // Assert
        Assert.Equal(expectedTrimmed, apiSecret.Value);
    }

    #endregion

    #region From Method - The vendor's own value is accepted

    /// <summary>
    /// The incident this rule was relaxed for: fiskaly issued a 42-character managed-organisation secret.
    /// It must be accepted, unchanged, because it is the credential fiskaly expects us to authenticate with.
    /// </summary>
    [Trait("Category", "Unit")]
    [Fact]
    public void From_TheFortyTwoCharacterSecretFiskalyActuallyIssued_IsAccepted()
    {
        string issuedByFiskaly = new string('a', 42);

        ApiSecret secret = ApiSecret.From(issuedByFiskaly);

        Assert.Equal(issuedByFiskaly, secret.Value);
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData(42)]
    [InlineData(43)]
    [InlineData(44)]
    [InlineData(100)]
    public void From_AnyPlausibleLength_IsAccepted(int length)
    {
        string value = new string('a', length);

        Assert.Equal(value, ApiSecret.From(value).Value);
    }

    /// <summary>
    /// Characters we do not control either. Underscores are in the format the old error message itself
    /// advertised while the old pattern rejected them; base64url secrets carry dashes and underscores.
    /// </summary>
    [Trait("Category", "Unit")]
    [Theory]
    [InlineData("test_api_secret_with_underscores_12345678")]
    [InlineData("test-api-secret-with-dashes-1234567890abc")]
    [InlineData("test.api.secret.with.dots.1234567890abcdef")]
    [InlineData("test!@#$%^&*()secret1234567890abcdefghijk")]
    public void From_CharactersFiskalyMayUse_AreAccepted(string issuedByFiskaly)
    {
        Assert.Equal(issuedByFiskaly, ApiSecret.From(issuedByFiskaly).Value);
    }

    #endregion

    #region From Method - Invalid Length

    [Trait("Category", "Unit")]
    [Fact]
    public void From_ApiSecretShorterThanTheMinimum_ThrowsFormatException()
    {
        // Short enough that it cannot be a credential at all - a truncated paste, not a vendor value.
        string invalidSecret = "abc";

        FormatException exception = Assert.Throws<FormatException>(() => ApiSecret.From(invalidSecret));
        Assert.Contains("at least 6 characters", exception.Message);
        Assert.Contains("Current: 3 characters", exception.Message);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_ApiSecretLongerThanTheMaximum_ThrowsFormatException()
    {
        // A whole file or JSON blob pasted into the field, rather than a credential.
        string invalidSecret = new string('x', 513);

        FormatException exception = Assert.Throws<FormatException>(() => ApiSecret.From(invalidSecret));
        Assert.Contains("must not exceed 512 characters", exception.Message);
        Assert.Contains("Current: 513 characters", exception.Message);
    }

    #endregion

    #region From Method - Null/Empty/Whitespace

    [Trait("Category", "Unit")]
    [Fact]
    public void From_NullApiSecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ApiSecret.From(null!));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_EmptyApiSecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ApiSecret.From(string.Empty));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void From_WhitespaceOnlyApiSecret_ThrowsArgumentException()
    {
        // Arrange: Only spaces
        string whitespaceSecret = "     ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ApiSecret.From(whitespaceSecret));
    }

    #endregion

    #region TryFrom Method - Valid Inputs

    [Trait("Category", "Unit")]
    [Fact]
    public void TryFrom_ValidApiSecret_ReturnsTrue()
    {
        // Arrange
        string validSecret = "abcdefghijklmnopqrstuvwxyz01234567890ABCDEF";

        // Act
        bool success = ApiSecret.TryFrom(validSecret, out ApiSecret apiSecret);

        // Assert
        Assert.True(success);
        Assert.Equal(validSecret, apiSecret.Value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TryFrom_ValidApiSecretWithWhitespace_ReturnsTrueAndTrims()
    {
        // Arrange
        string secretWithWhitespace = "  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA  ";
        string expectedTrimmed = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        // Act
        bool success = ApiSecret.TryFrom(secretWithWhitespace, out ApiSecret apiSecret);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedTrimmed, apiSecret.Value);
    }

    #endregion

    #region TryFrom Method - Invalid Inputs

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryFrom_NullOrWhitespace_ReturnsFalse(string? invalidSecret)
    {
        // Act
        bool success = ApiSecret.TryFrom(invalidSecret, out ApiSecret apiSecret);

        // Assert
        Assert.False(success);
        Assert.Equal(default, apiSecret);
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData("abc")]                  // too short to be a credential at all
    [InlineData("")]                     // nothing at all
    public void TryFrom_OutsideThePlausibleBand_ReturnsFalse(string invalidSecret)
    {
        bool success = ApiSecret.TryFrom(invalidSecret, out ApiSecret apiSecret);

        Assert.False(success);
        Assert.Equal(default, apiSecret);
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData("test-secret-with-dashes-1234567890abcdefg")]
    [InlineData("test_secret_with_underscores_12345678901")]
    [InlineData("test!@#secret$%^with&*()special1234567890")]
    public void TryFrom_CharactersFiskalyMayUse_ReturnsTrue(string issuedByFiskaly)
    {
        bool success = ApiSecret.TryFrom(issuedByFiskaly, out ApiSecret apiSecret);

        Assert.True(success);
        Assert.Equal(issuedByFiskaly, apiSecret.Value);
    }

    #endregion

    #region ToString Method (Security)

    [Trait("Category", "Unit")]
    [Fact]
    public void ToString_MasksSecretValue()
    {
        // Arrange
        string validSecret = "abcdefghijklmnopqrstuvwxyz01234567890ABCDEF";
        ApiSecret apiSecret = ApiSecret.From(validSecret);

        // Act
        string stringValue = apiSecret.ToString();

        // Assert
        Assert.DoesNotContain(validSecret, stringValue);
        Assert.Equal("********", stringValue); // Should be masked (8 asterisks)
    }

    #endregion

    #region Equality and Record Behavior

    [Trait("Category", "Unit")]
    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        ApiSecret secret1 = ApiSecret.From("abcdefghijklmnopqrstuvwxyz01234567890ABCDEF");
        ApiSecret secret2 = ApiSecret.From("abcdefghijklmnopqrstuvwxyz01234567890ABCDEF");

        // Act & Assert
        Assert.Equal(secret1, secret2);
        Assert.True(secret1 == secret2);
        Assert.False(secret1 != secret2);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        ApiSecret secret1 = ApiSecret.From("abcdefghijklmnopqrstuvwxyz01234567890ABCDEF");
        ApiSecret secret2 = ApiSecret.From("differentSecretValue1234567890ABCDEFGHIJKLM");

        // Act & Assert
        Assert.NotEqual(secret1, secret2);
        Assert.False(secret1 == secret2);
        Assert.True(secret1 != secret2);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void GetHashCode_SameValue_SameHashCode()
    {
        // Arrange
        ApiSecret secret1 = ApiSecret.From("abcdefghijklmnopqrstuvwxyz01234567890ABCDEF");
        ApiSecret secret2 = ApiSecret.From("abcdefghijklmnopqrstuvwxyz01234567890ABCDEF");

        // Act & Assert
        Assert.Equal(secret1.GetHashCode(), secret2.GetHashCode());
    }

    #endregion

    #region IParsable - Parse Method

    [Trait("Category", "Unit")]
    [Fact]
    public void Parse_WithValidInput_ReturnsInstance()
    {
        // Arrange
        string validInput = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"; // 43 chars

        // Act
        ApiSecret result = ApiSecret.Parse(validInput, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validInput, result.Value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Parse_WithNullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ApiSecret.Parse(null!, null));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Parse_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ApiSecret.Parse(string.Empty, null));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Parse_WithWhitespace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ApiSecret.Parse("   ", null));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Parse_WithSomethingTooShortToBeACredential_ThrowsFormatException()
    {
        string invalidInput = "abc";

        Assert.Throws<FormatException>(() => ApiSecret.Parse(invalidInput, null));
    }

    #endregion

    #region IParsable - TryParse Method

    [Trait("Category", "Unit")]
    [Fact]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        // Arrange
        string validInput = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        // Act
        bool success = ApiSecret.TryParse(validInput, null, out ApiSecret result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(validInput, result.Value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TryParse_WithNullInput_ReturnsFalse()
    {
        // Act
        bool success = ApiSecret.TryParse(null, null, out ApiSecret result);

        // Assert
        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TryParse_WithInvalidInput_ReturnsFalse()
    {
        // Arrange
        string invalidInput = "";

        // Act
        bool success = ApiSecret.TryParse(invalidInput, null, out ApiSecret result);

        // Assert
        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TryParse_WithSomethingTooShortToBeACredential_ReturnsFalse()
    {
        string invalidInput = "abc";

        bool success = ApiSecret.TryParse(invalidInput, null, out ApiSecret result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TryParse_WithCharactersFiskalyMayUse_ReturnsTrue()
    {
        string issuedByFiskaly = "test-secret-with-dashes-1234567890abcdefg";

        bool success = ApiSecret.TryParse(issuedByFiskaly, null, out ApiSecret result);

        Assert.True(success);
        Assert.Equal(issuedByFiskaly, result.Value);
    }

    #endregion
}
