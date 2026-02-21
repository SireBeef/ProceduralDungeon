using MonoGameLibrary.WFC.Core;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public enum TestTile { Empty, Floor, Wall }

public class WFCAdjacencyRulesTests
{
    [Fact]
    public void AddRule_WhenRuleAdded_GetAllowedNeighborsReturnsIt()
    {
        var rules = new WFCAdjacencyRules<TestTile>();

        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);

        var allowed = rules.GetAllowedNeighbors(TestTile.Floor, Direction.North);
        Assert.Contains(TestTile.Floor, allowed);
    }

    [Fact]
    public void AddRule_WhenMultipleNeighborsAdded_GetAllowedNeighborsReturnsAll()
    {
        var rules = new WFCAdjacencyRules<TestTile>();

        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Wall);

        var allowed = rules.GetAllowedNeighbors(TestTile.Floor, Direction.North);
        Assert.Equal(2, allowed.Count);
        Assert.Contains(TestTile.Floor, allowed);
        Assert.Contains(TestTile.Wall, allowed);
    }

    [Fact]
    public void GetAllowedNeighbors_WhenNoRulesForTile_ReturnsEmptySet()
    {
        var rules = new WFCAdjacencyRules<TestTile>();

        var allowed = rules.GetAllowedNeighbors(TestTile.Empty, Direction.North);

        Assert.Empty(allowed);
    }

    [Fact]
    public void GetAllowedNeighbors_WhenNoRulesForDirection_ReturnsEmptySet()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);

        var allowed = rules.GetAllowedNeighbors(TestTile.Floor, Direction.South);

        Assert.Empty(allowed);
    }

    [Fact]
    public void IsAllowed_WhenRuleExists_ReturnsTrue()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        rules.AddRule(TestTile.Floor, Direction.East, TestTile.Wall);

        Assert.True(rules.IsAllowed(TestTile.Floor, Direction.East, TestTile.Wall));
    }

    [Fact]
    public void IsAllowed_WhenRuleDoesNotExist_ReturnsFalse()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        rules.AddRule(TestTile.Floor, Direction.East, TestTile.Wall);

        Assert.False(rules.IsAllowed(TestTile.Floor, Direction.East, TestTile.Empty));
    }

    [Fact]
    public void AddRule_WhenDuplicateAdded_DoesNotDuplicate()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);

        var allowed = rules.GetAllowedNeighbors(TestTile.Floor, Direction.North);
        Assert.Single(allowed);
    }
}
