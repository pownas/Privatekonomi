using Microsoft.Extensions.Caching.Memory;
using Privatekonomi.Core.Services;
using Xunit;

namespace Privatekonomi.Core.Tests;

public class OAuthStateServiceTests
{
    private readonly IOAuthStateService _stateService;

    public OAuthStateServiceTests()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        _stateService = new OAuthStateService(cache);
    }

    [Fact]
    public void GenerateState_ShouldReturnNonEmptyString()
    {
        // Arrange
        var bankName = "Swedbank";

        // Act
        var state = _stateService.GenerateState(bankName);

        // Assert
        Assert.NotNull(state);
        Assert.NotEmpty(state);
    }

    [Fact]
    public void GenerateState_ShouldReturnUniqueStates()
    {
        // Arrange
        var bankName = "Swedbank";

        // Act
        var state1 = _stateService.GenerateState(bankName);
        var state2 = _stateService.GenerateState(bankName);

        // Assert
        Assert.NotEqual(state1, state2);
    }

    [Fact]
    public void ValidateState_ValidState_ShouldReturnTrue()
    {
        // Arrange
        var bankName = "Swedbank";
        var state = _stateService.GenerateState(bankName);

        // Act
        var isValid = _stateService.ValidateState(state, bankName);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateState_InvalidState_ShouldReturnFalse()
    {
        // Arrange
        var bankName = "Swedbank";
        var invalidState = "invalid-state-12345";

        // Act
        var isValid = _stateService.ValidateState(invalidState, bankName);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateState_WrongBankName_ShouldReturnFalse()
    {
        // Arrange
        var bankName = "Swedbank";
        var state = _stateService.GenerateState(bankName);

        // Act
        var isValid = _stateService.ValidateState(state, "ICA-banken");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateState_EmptyState_ShouldReturnFalse()
    {
        // Arrange & Act
        var isValid = _stateService.ValidateState(string.Empty, "Swedbank");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void RemoveState_ShouldInvalidateState()
    {
        // Arrange
        var bankName = "Swedbank";
        var state = _stateService.GenerateState(bankName);

        // Act
        _stateService.RemoveState(state);
        var isValid = _stateService.ValidateState(state, bankName);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void RemoveState_NonExistentState_ShouldNotThrow()
    {
        // Arrange
        var nonExistentState = "non-existent-state";

        // Act & Assert
        var exception = Record.Exception(() => _stateService.RemoveState(nonExistentState));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateState_CaseInsensitiveBankName_ShouldWork()
    {
        // Arrange
        var state = _stateService.GenerateState("Swedbank");

        // Act
        var isValid1 = _stateService.ValidateState(state, "swedbank");
        var isValid2 = _stateService.ValidateState(state, "SWEDBANK");

        // Assert
        Assert.True(isValid1);
        Assert.True(isValid2);
    }

    [Fact]
    public void StateLifecycle_GenerateValidateRemove_ShouldWork()
    {
        // Arrange
        var bankName = "ICA-banken";

        // Act - Generate
        var state = _stateService.GenerateState(bankName);
        Assert.NotNull(state);

        // Act - Validate (should be valid)
        var isValidBefore = _stateService.ValidateState(state, bankName);
        Assert.True(isValidBefore);

        // Act - Remove
        _stateService.RemoveState(state);

        // Act - Validate again (should be invalid after removal)
        var isValidAfter = _stateService.ValidateState(state, bankName);
        Assert.False(isValidAfter);
    }
}
