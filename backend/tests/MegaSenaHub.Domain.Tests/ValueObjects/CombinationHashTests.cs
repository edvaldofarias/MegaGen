using FluentAssertions;
using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Tests.ValueObjects;

public sealed class CombinationHashTests
{
    [Fact]
    public void CreateFromNumbers_ShouldReturnOrderedHash_WhenNumbersAreValid()
    {
        // Arrange — números fora de ordem propositalmente
        var numbers = new[] { 6, 3, 1, 4, 2, 5 };

        // Act
        var hash = CombinationHash.CreateFromNumbers(numbers);

        // Assert
        hash.Value.Should().Be("01-02-03-04-05-06");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsEmpty()
    {
        // Arrange & Act
        Action act = () => CombinationHash.Create(string.Empty);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("1-2-3-4-5-6")]        // dígito único por número
    [InlineData("01-02-03-04-05")]      // apenas 5 grupos
    [InlineData("01:02:03:04:05:06")]   // separador errado
    [InlineData("01-02-03-04-05-06-07")] // 7 grupos
    public void Create_ShouldThrowDomainException_WhenFormatIsInvalid(string value)
    {
        // Arrange & Act
        Action act = () => CombinationHash.Create(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldCreateHash_WhenFormatIsValid()
    {
        // Arrange
        const string valid = "10-20-30-40-50-60";

        // Act
        var hash = CombinationHash.Create(valid);

        // Assert
        hash.Value.Should().Be(valid);
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenTwoHashesHaveSameValue()
    {
        // Arrange
        var a = CombinationHash.Create("01-02-03-04-05-06");
        var b = CombinationHash.Create("01-02-03-04-05-06");

        // Assert
        a.Should().Be(b);
    }
}
