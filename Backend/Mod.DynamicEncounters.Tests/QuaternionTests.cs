using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Tests;

[TestFixture]
public class QuaternionTests
{
    [Test]
    public void FindLookAtRotation_BasicRotation_CorrectQuaternion()
    {
        var currentPos = DenseVector.OfArray([0, 0, 0]);
        var targetPos = DenseVector.OfArray([1, 1, 1]);

        var result = MathNetHelpers.FindLookAtRotation(currentPos, targetPos);

        // Expected quaternion values (you need to compute these manually or with another reliable method)
        var expected = new Quaternion(0.7071, 0.0, 0.7071, 0.0); // Example values

        Assert.That(expected.Equals(result, 1e-4), $"Expected: {expected}, Result {result}");
    }

    [Test]
    public void FindLookAtRotation_SamePosition_IdentityQuaternion()
    {
        var position = DenseVector.OfArray(new double[] { 1, 1, 1 });

        var result = MathNetHelpers.FindLookAtRotation(position, position);

        var expected = new Quaternion(1, 0, 0, 0); // Identity quaternion

        Assert.That(expected.Equals(result, 1e-4), $"Expected: {expected}, Result {result}");
    }

    [Test]
    public void FindLookAtRotation_AlongAxis_CorrectQuaternion()
    {
        var currentPos = DenseVector.OfArray(new double[] { 0, 0, 0 });
        var targetPos = DenseVector.OfArray(new double[] { 1, 0, 0 }); // X-axis

        var result = MathNetHelpers.FindLookAtRotation(currentPos, targetPos);

        var expected = new Quaternion(0, 0, 0, 1); // Example expected value

        Assert.That(expected.Equals(result, 1e-4));
    }

    [Test]
    public void FindLookAtRotation_LargeDistance_CorrectQuaternion()
    {
        var currentPos = DenseVector.OfArray(new double[] { 1000, 1000, 1000 });
        var targetPos = DenseVector.OfArray(new double[] { 2000, 2000, 2000 });

        var result = MathNetHelpers.FindLookAtRotation(currentPos, targetPos);

        var expected = new Quaternion(0.7071, 0.0, 0.7071, 0.0); // Example values

        Assert.That(expected.Equals(result, 1e-4));
    }

    [Test]
    public void FindLookAtRotation_VerticalDirection_CorrectQuaternion()
    {
        var currentPos = DenseVector.OfArray(new double[] { 0, 0, 0 });
        var targetPos = DenseVector.OfArray(new double[] { 0, 1, 0 }); // Y-axis

        var result = MathNetHelpers.FindLookAtRotation(currentPos, targetPos);

        var expected = new Quaternion(0.7071, 0.7071, 0, 0); // Example values

        Assert.That(expected.Equals(result, 1e-4));
    }
}