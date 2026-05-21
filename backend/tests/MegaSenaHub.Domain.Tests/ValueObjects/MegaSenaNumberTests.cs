using FluentAssertions;
using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Tests.ValueObjects;

public sealed class MegaSenaNumberTests
{
    [Fact]
    public void Create_ShouldCreateNumber_WhenValueIsBetweenOneAndSixty()
    {
        // Arrange
        const int value = 30;

        // Act
        var number = MegaSenaNumber.Create(value);

        // Assert
        number.Value.Should().Be(30);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ShouldThrowDomainException_WhenValueIsLessThanOne(int value)
    {
        // Arrange & Act
        Action act = () => MegaSenaNumber.Create(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(61)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Create_ShouldThrowDomainException_WhenValueIsGreaterThanSixty(int value)
    {
        // Arrange & Act
        Action act = () => MegaSenaNumber.Create(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1, "01")]
    [InlineData(9, "09")]
    [InlineData(5, "05")]
    public void ToString_ShouldReturnTwoDigits_WhenValueHasOneDigit(int value, string expected)
    {
        // Arrange
        var number = MegaSenaNumber.Create(value);

        // Act
        var result = number.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, "10")]
    [InlineData(42, "42")]
    [InlineData(60, "60")]
    public void ToString_ShouldReturnValue_WhenValueHasTwoDigits(int value, string expected)
    {
        // Arrange
        var number = MegaSenaNumber.Create(value);

        // Act
        var result = number.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenTwoNumbersHaveSameValue()
    {
        // Arrange
        var a = MegaSenaNumber.Create(15);
        var b = MegaSenaNumber.Create(15);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenTwoNumbersHaveDifferentValues()
    {
        // Arrange
        var a = MegaSenaNumber.Create(15);
        var b = MegaSenaNumber.Create(16);

        // Assert
        a.Should().NotBe(b);
    }
}
