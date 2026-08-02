using Bolt.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class ModificationConflictOrderTests
{
    [TestMethod]
    public void SettingLeftBeforeAutomaticallyMakesRightAfter()
    {
        var left = Guid.NewGuid();
        var middle = Guid.NewGuid();
        var right = Guid.NewGuid();
        var order = new ModificationConflictOrder([right, middle, left]);

        order.SetPosition(left, right, ConflictPosition.Before);

        Assert.AreEqual(ConflictPosition.Before, order.GetPosition(left, right));
        Assert.AreEqual(ConflictPosition.After, order.GetPosition(right, left));
        CollectionAssert.AreEqual(new[] { left, right, middle }, order.ModificationIds.ToArray());
    }

    [TestMethod]
    public void SettingRightBeforeAutomaticallyMakesLeftAfter()
    {
        var left = Guid.NewGuid();
        var right = Guid.NewGuid();
        var order = new ModificationConflictOrder([left, right]);

        order.SetPosition(right, left, ConflictPosition.Before);

        Assert.AreEqual(ConflictPosition.After, order.GetPosition(left, right));
        Assert.AreEqual(ConflictPosition.Before, order.GetPosition(right, left));
        CollectionAssert.AreEqual(new[] { right, left }, order.ModificationIds.ToArray());
    }
}
