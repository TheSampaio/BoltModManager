using System.Drawing;
using Bolt.Core.Models;
using Bolt.UI.Controls;
using Bolt.UI.Theme;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class GridFrameTests
{
    [TestMethod]
    public void DarkThemeUsesRequestedGridBorderColor()
    {
        AppTheme.Apply(ThemeMode.Dark);

        Assert.AreEqual(Color.FromArgb(0x30, 0x33, 0x38), GridFrame.BorderColor);
    }
}
