using MonoGameLibrary.WFC.Core;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public class WFCOverlappingModelTests
{
    [Fact]
    public void UniformInput_ProducesUniformOutput()
    {
        var input = new TestTile[4, 4];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                input[x, y] = TestTile.Floor;

        var model = new WFCOverlappingModel<TestTile>(input, 2, 10, 10);
        bool success = model.Run(42);

        Assert.True(success);
        var output = model.GetOutput();
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                Assert.Equal(TestTile.Floor, output[x, y]);
    }

    [Fact]
    public void CheckerboardInput_ProducesCheckerboardOutput()
    {
        var input = new TestTile[4, 4];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                input[x, y] = (x + y) % 2 == 0 ? TestTile.Floor : TestTile.Empty;

        var model = new WFCOverlappingModel<TestTile>(input, 2, 10, 10);
        bool success = model.Run(42);

        Assert.True(success);
        var output = model.GetOutput();

        // Every cell should have a neighbor of the opposite type
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                var tile = output[x, y];
                // In a valid checkerboard, adjacent cells differ
                if (x + 1 < 10)
                    Assert.NotEqual(tile, output[x + 1, y]);
                if (y + 1 < 10)
                    Assert.NotEqual(tile, output[x, y + 1]);
            }
        }
    }

    [Fact]
    public void SameSeed_ProducesSameOutput()
    {
        var input = new TestTile[4, 4];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                input[x, y] = (x + y) % 2 == 0 ? TestTile.Floor : TestTile.Empty;

        var model1 = new WFCOverlappingModel<TestTile>(input, 2, 10, 10);
        model1.Run(123);
        var output1 = model1.GetOutput();

        var model2 = new WFCOverlappingModel<TestTile>(input, 2, 10, 10);
        model2.Run(123);
        var output2 = model2.GetOutput();

        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                Assert.Equal(output1[x, y], output2[x, y]);
    }

    [Fact]
    public void DifferentSeeds_CanProduceDifferentOutputs()
    {
        var input = new TestTile[6, 6];
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                input[x, y] = y < 3 ? TestTile.Floor : TestTile.Empty;

        bool foundDifference = false;
        for (int seed = 0; seed < 20; seed++)
        {
            var model1 = new WFCOverlappingModel<TestTile>(input, 2, 10, 10, symmetry: 1);
            var model2 = new WFCOverlappingModel<TestTile>(input, 2, 10, 10, symmetry: 1);

            if (!model1.Run(seed) || !model2.Run(seed + 100)) continue;

            var out1 = model1.GetOutput();
            var out2 = model2.GetOutput();

            for (int x = 0; x < 10 && !foundDifference; x++)
                for (int y = 0; y < 10 && !foundDifference; y++)
                    if (!out1[x, y].Equals(out2[x, y]))
                        foundDifference = true;

            if (foundDifference) break;
        }

        Assert.True(foundDifference);
    }

    [Fact]
    public void HorizontalStripes_WithSymmetry1_ProducesHorizontalStripes()
    {
        // Horizontal stripes: rows of same tile
        var input = new TestTile[6, 6];
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                input[x, y] = y % 2 == 0 ? TestTile.Floor : TestTile.Empty;

        var model = new WFCOverlappingModel<TestTile>(input, 2, 8, 8, symmetry: 1);
        bool success = model.Run(42);

        Assert.True(success);
        var output = model.GetOutput();

        // Each row should be uniform (all same tile)
        for (int y = 0; y < 8; y++)
        {
            var firstTile = output[0, y];
            for (int x = 1; x < 8; x++)
                Assert.Equal(firstTile, output[x, y]);
        }
    }

    [Fact]
    public void PatternCount_ReflectsExtractedPatterns()
    {
        var input = new TestTile[4, 4];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                input[x, y] = TestTile.Floor;

        var model = new WFCOverlappingModel<TestTile>(input, 2, 5, 5);
        Assert.True(model.PatternCount > 0);
    }

    [Fact]
    public void Run_CanBeCalledMultipleTimes()
    {
        var input = new TestTile[4, 4];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                input[x, y] = TestTile.Floor;

        var model = new WFCOverlappingModel<TestTile>(input, 2, 8, 8);

        bool success1 = model.Run(1);
        Assert.True(success1);
        var output1 = model.GetOutput();

        bool success2 = model.Run(2);
        Assert.True(success2);
        var output2 = model.GetOutput();

        // Both should succeed (uniform input)
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                Assert.Equal(TestTile.Floor, output2[x, y]);
    }
}
