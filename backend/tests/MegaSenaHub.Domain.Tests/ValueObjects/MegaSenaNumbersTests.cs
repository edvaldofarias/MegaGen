using FluentAssertions;
using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Tests.ValueObjects;

public sealed class MegaSenaNumbersTests
{
    [Fact]
    public void Create_ShouldCreateOrderedNumbers_WhenNumbersAreValid()
    {
        // Arrange
        var rawNumbers = new[] { 6, 3, 1, 4, 2, 5 };

        // Act
        var numbers = MegaSenaNumbers.Create(rawNumbers);

        // Assert
        numbers.AsReadOnly()
            .Select(n => n.Value)
            .Should().BeInAscendingOrder();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenNumbersContainDuplicates()
    {
        // Arrange
        var duplicated = new[] { 1, 2, 3, 4, 5, 5 };

        // Act
        Action act = () => MegaSenaNumbers.Create(duplicated);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenNumbersCountIsLessThanSix()
    {
        // Arrange
        var tooFew = new[] { 1, 2, 3, 4, 5 };

        // Act
        Action act = () => MegaSenaNumbers.Create(tooFew);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenNumbersCountIsGreaterThanSix()
    {
        // Arrange
        var tooMany = new[] { 1, 2, 3, 4, 5, 6, 7 };

        // Act
        Action act = () => MegaSenaNumbers.Create(tooMany);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ToCombinationHash_ShouldReturnOrderedHash_WhenNumbersAreUnordered()
    {
        // Arrange
        var numbers = MegaSenaNumbers.Create(new[] { 6, 3, 1, 4, 2, 5 });

        // Act
        var hash = numbers.ToCombinationHash();

        // Assert
        hash.Value.Should().Be("01-02-03-04-05-06");
    }

    [Fact]
    public void CountHits_ShouldReturnCorrectHits_WhenComparedWithAnotherCombination()
    {
        // Arrange — quatro números em comum
        var drawn = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 5, 6 });
        var bet = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 7, 8 });

        // Act
        var hits = drawn.CountHits(bet);

        // Assert
        hits.Should().Be(4);
    }

    [Fact]
    public void CountHits_ShouldReturnZero_WhenNoNumbersMatch()
    {
        // Arrange
        var drawn = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 5, 6 });
        var bet = MegaSenaNumbers.Create(new[] { 7, 8, 9, 10, 11, 12 });

        // Act
        var hits = drawn.CountHits(bet);

        // Assert
        hits.Should().Be(0);
    }

    [Fact]
    public void CountHits_ShouldReturnSix_WhenAllNumbersMatch()
    {
        // Arrange
        var drawn = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 5, 6 });
        var bet = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 5, 6 });

        // Act
        var hits = drawn.CountHits(bet);

        // Assert
        hits.Should().Be(6);
    }
}
