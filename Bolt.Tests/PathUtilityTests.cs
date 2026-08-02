using Bolt.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class PathUtilityTests
{
    [TestMethod]
    public void RebaseFolderFilePreservesSelectedFolderAndDescendantDirectories()
    {
        var result = PathUtility.RebaseFolderFile(
            Path.Combine("Shaders", "resources", "example.png"),
            "Shaders",
            "modloader");

        Assert.AreEqual(
            Path.Combine("modloader", "Shaders", "resources", "example.png"),
            result);
    }

    [TestMethod]
    public void RebaseFolderFilePreservesOnlyTheSelectedSubtree()
    {
        var result = PathUtility.RebaseFolderFile(
            Path.Combine("Shaders", "resources", "textures", "example.png"),
            Path.Combine("Shaders", "resources"),
            "modloader");

        Assert.AreEqual(
            Path.Combine("modloader", "resources", "textures", "example.png"),
            result);
    }
}
