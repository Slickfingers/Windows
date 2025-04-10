// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Tests;
using CommunityToolkit.Tooling.TestGen;
using CommunityToolkit.WinUI.Controls;

namespace PrimitivesTests;

[TestClass]
public partial class Test_UniformGrid_AutoLayout : VisualUITestBase
{
    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_FixedElementSingle()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border/>
        <Border Grid.Row=""1"" Grid.Column=""1""/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        var expected = new (int row, int col)[]
        {
                (0, 0),
                (1, 1),
                (0, 1),
                (0, 2),
                (1, 0),
                (1, 2),
                (2, 0),
                (2, 1)
        };

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement).ToArray();

        Assert.AreEqual(8, grid.Children.Count);

        grid.Measure(new Size(1000, 1000));

        // Check all children are in expected places.
        for (int i = 0; i < children.Count(); i++)
        {
            if (expected[i].row == 1 && expected[i].col == 1)
            {
                // Check our fixed item isn't set to auto-layout.
                Assert.AreEqual(false, UniformGrid.GetAutoLayout(children[i]!));
            }

            Assert.AreEqual(expected[i].row, Grid.GetRow(children[i]));
            Assert.AreEqual(expected[i].col, Grid.GetColumn(children[i]));
        }
    }

    /// <summary>
    /// Note: This one particular special-case scenario requires 16299 for the <see cref="MarkupExtension"/>.
    /// </summary>
    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_FixedElementZeroZeroSpecial(AutoLayoutFixedElementZeroZeroSpecialPage treeRoot)
    {
        var expected = new (int row, int col)[]
        {
                (0, 1),
                (0, 2),
                (1, 0),
                (1, 1),
                (1, 2),
                (2, 0),
                (0, 0),
                (2, 1)
        };

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement).ToArray();

        Assert.AreEqual(8, grid.Children.Count);

        grid.Measure(new Size(1000, 1000));

        // Check all children are in expected places.
        for (int i = 0; i < children.Count(); i++)
        {
            if (expected[i].row == 0 && expected[i].col == 0)
            {
                // Check our fixed item isn't set to auto-layout.
                Assert.AreEqual(false, UniformGrid.GetAutoLayout(children[i]!));
            }

            Assert.AreEqual(expected[i].row, Grid.GetRow(children[i]));
            Assert.AreEqual(expected[i].col, Grid.GetColumn(children[i]));
        }
    }

    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_FixedElementSquare()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border/>
        <Border Grid.Row=""1"" Grid.Column=""1"" Grid.RowSpan=""2"" Grid.ColumnSpan=""2""/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        var expected = new (int row, int col)[]
        {
                (0, 0),
                (1, 1),
                (0, 1),
                (0, 2),
                (0, 3),
                (1, 0),
                (1, 3),
                (2, 0),
                (2, 3)
        };

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement).ToArray();

        Assert.AreEqual(9, grid.Children.Count);

        grid.Measure(new Size(1000, 1000));

        // Check all children are in expected places.
        for (int i = 0; i < children.Count(); i++)
        {
            if (expected[i].row == 1 && expected[i].col == 1)
            {
                // Check our fixed item isn't set to auto-layout.
                Assert.AreEqual(false, UniformGrid.GetAutoLayout(children[i]!));
            }

            Assert.AreEqual(expected[i].row, Grid.GetRow(children[i]));
            Assert.AreEqual(expected[i].col, Grid.GetColumn(children[i]));
        }
    }

    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_VerticalElement_FixedPosition()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border/>
        <Border Grid.Row=""1"" Grid.Column=""1"" Grid.RowSpan=""2"" x:Name=""OurItem""/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border x:Name=""Shifted""/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement);

        Assert.AreEqual(8, grid.Children.Count());

        grid.Measure(new Size(1000, 1000));

        var border = treeRoot.FindChild("OurItem") as Border;

        Assert.IsNotNull(border, "Could not find our item to test.");

        Assert.AreEqual(1, Grid.GetRow(border));
        Assert.AreEqual(1, Grid.GetColumn(border));

        var border2 = treeRoot.FindChild("Shifted") as Border;

        Assert.IsNotNull(border2, "Could not find shifted item to test.");

        Assert.AreEqual(2, Grid.GetRow(border2));
        Assert.AreEqual(2, Grid.GetColumn(border2));
    }

    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_VerticalElement()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border Grid.RowSpan=""2"" x:Name=""OurItem""/>
        <Border/>
        <Border/>
        <Border x:Name=""Shifted""/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement);

        Assert.AreEqual(8, grid.Children.Count());

        grid.Measure(new Size(1000, 1000));

        var border = treeRoot.FindChild("OurItem") as Border;

        Assert.IsNotNull(border, "Could not find our item to test.");

        Assert.AreEqual(1, Grid.GetRow(border));
        Assert.AreEqual(1, Grid.GetColumn(border));

        var border2 = treeRoot.FindChild("Shifted") as Border;

        Assert.IsNotNull(border2, "Could not find shifted item to test.");

        Assert.AreEqual(2, Grid.GetRow(border2));
        Assert.AreEqual(2, Grid.GetColumn(border2));
    }

    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_HorizontalElement()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border/>
        <Border Grid.ColumnSpan=""2"" x:Name=""OurItem""/>
        <Border x:Name=""Shifted""/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement);

        Assert.AreEqual(8, grid.Children.Count());

        grid.Measure(new Size(1000, 1000));

        var border = treeRoot.FindChild("OurItem") as Border;

        Assert.IsNotNull(border, "Could not find our item to test.");

        Assert.AreEqual(0, Grid.GetRow(border));
        Assert.AreEqual(1, Grid.GetColumn(border));

        var border2 = treeRoot.FindChild("Shifted") as Border;

        Assert.IsNotNull(border2, "Could not find shifted item to test.");

        Assert.AreEqual(1, Grid.GetRow(border2));
        Assert.AreEqual(0, Grid.GetColumn(border2));
    }

    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_LargeElement()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border Grid.ColumnSpan=""2"" Grid.RowSpan=""2""/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
        <Border/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        var expected = new (int row, int col)[]
        {
                (0, 0),
                (0, 2),
                (1, 2),
                (2, 0),
                (2, 1),
                (2, 2),
        };

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement).ToArray();

        Assert.AreEqual(6, grid.Children.Count());

        grid.Measure(new Size(1000, 1000));

        // Check all children are in expected places.
        for (int i = 0; i < children.Count(); i++)
        {
            Assert.AreEqual(expected[i].row, Grid.GetRow(children[i]));
            Assert.AreEqual(expected[i].col, Grid.GetColumn(children[i]));
        }
    }

    [TestCategory("UniformGrid")]
    [UIThreadTestMethod]
    public void Test_UniformGrid_AutoLayout_HorizontalElement_FixedPosition()
    {
        var treeRoot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:controls=""using:CommunityToolkit.WinUI.Controls"">
    <controls:UniformGrid x:Name=""UniformGrid"">
        <Border/>
        <Border Grid.Row=""1"" Grid.Column=""1"" Grid.ColumnSpan=""2"" x:Name=""OurItem""/>
        <Border/>
        <Border/>
        <Border/>
        <Border x:Name=""Shifted""/>
        <Border/>
        <Border/>
    </controls:UniformGrid>
</Page>") as FrameworkElement;

        Assert.IsNotNull(treeRoot, "Could not load XAML tree.");

        var grid = treeRoot.FindChild("UniformGrid") as UniformGrid;

        Assert.IsNotNull(grid, "Could not find UniformGrid in tree.");

        var children = grid.Children.Select(item => item as FrameworkElement);

        Assert.AreEqual(8, grid.Children.Count());

        grid.Measure(new Size(1000, 1000));

        var border = treeRoot.FindChild("OurItem") as Border;

        Assert.IsNotNull(border, "Could not find our item to test.");

        Assert.AreEqual(1, Grid.GetRow(border));
        Assert.AreEqual(1, Grid.GetColumn(border));

        var border2 = treeRoot.FindChild("Shifted") as Border;

        Assert.IsNotNull(border2, "Could not find shifted item to test.");

        Assert.AreEqual(2, Grid.GetRow(border2));
        Assert.AreEqual(0, Grid.GetColumn(border2));
    }
}
