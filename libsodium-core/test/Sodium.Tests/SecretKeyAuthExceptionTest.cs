using System.Text;
using NUnit.Framework;
using Sodium;
using Sodium.Exceptions;

namespace Tests
{
  /// <summary>Exception tests for the SecretKeyAuth class</summary>
  [TestFixture]
  public class SecretKeyAuthExceptionTest
  {
    [Test]
    public void SecretKeyAuthSignWithBadKey()
    {
      Assert.Throws<KeyOutOfRangeException>(() =>
      {
        SecretKeyAuth.Sign(Encoding.UTF8.GetBytes("Adam Caudill"),
          Encoding.UTF8.GetBytes("0123456789012345678901234567890"));
      });
    }

    [Test]
    public void SecretKeyAuthSign256WithBadKey()
    {
      Assert.Throws<KeyOutOfRangeException>(() =>
      {
        SecretKeyAuth.SignHmacSha256(Encoding.UTF8.GetBytes("Adam Caudill"),
          Encoding.UTF8.GetBytes("012345678901234567890123456789"));
      });
    }

    [Test]
    public void SecretKeyAuthSign512WithBadKey()
    {
      Assert.Throws<KeyOutOfRangeException>(() =>
      {
        SecretKeyAuth.SignHmacSha512(Encoding.UTF8.GetBytes("Adam Caudill"),
          Encoding.UTF8.GetBytes("012345678901234567890123456789"));
      });

    }

    [Test]
    public void SecretKeyAuthVerifyWithBadKey()
    {
      Assert.Throws<KeyOutOfRangeException>(() =>
      {
        SecretKeyAuth.Verify(Encoding.UTF8.GetBytes("Adam Caudill"),
          Utilities.HexToBinary("9f44681a662b7cde80c4eb34db5102b62a8b482272e3cceef73a334ec1d321c0"),
          Encoding.UTF8.GetBytes("012345678901234567890123456789"));
      });

    }

    [Test]
    public void SecretKeyAuthVerifyWithBadSignature()
    {
      Assert.Throws<SignatureOutOfRangeException>(() =>
      {
        SecretKeyAuth.Verify(Encoding.UTF8.GetBytes("Adam Caudill"),
          Utilities.HexToBinary("9f44681a662b7cde80c4eb34db5102b62a8b482272e3cceef73a334ec1d321"),
          Encoding.UTF8.GetBytes("01234567890123456789012345678901"));
      });

    }

    [Test]
    public void SecretKeyAuthVerify256WithBadKey()
    {
      Assert.Throws<KeyOutOfRangeException>(() =>
      {
        SecretKeyAuth.VerifyHmacSha256(Encoding.UTF8.GetBytes("Adam Caudill"),
          Utilities.HexToBinary("9f44681a662b7cde80c4eb34db5102b62a8b482272e3cceef73a334ec1d321c0"),
          Encoding.UTF8.GetBytes("012345678901234567890123456789"));
      });

    }

    [Test]
    public void SecretKeyAuthVerify256WithBadSignature()
    {
      Assert.Throws<SignatureOutOfRangeException>(() =>
      {
        SecretKeyAuth.VerifyHmacSha256(Encoding.UTF8.GetBytes("Adam Caudill"),
          Utilities.HexToBinary("9f44681a662b7cde80c4eb34db5102b62a8b482272e3cceef73a334ec1d321"),
          Encoding.UTF8.GetBytes("01234567890123456789012345678901"));
      });

    }

    [Test]
    public void SecretKeyAuthVerify512WithBadKey()
    {
      Assert.Throws<KeyOutOfRangeException>(() =>
      {
        SecretKeyAuth.VerifyHmacSha512(Encoding.UTF8.GetBytes("Adam Caudill"),
          Utilities.HexToBinary(
            "9f44681a662b7cde80c4eb34db5102b62a8b482272e3cceef73a334ec1d321c06a99b828e2ff921b4d1304bbd9480adfacf8c4c2ffbcbb4e5663446fda1235d2"),
          Encoding.UTF8.GetBytes("012345678901234567890123456789"));
      });

    }

    [Test]
    public void SecretKeyAuthVerify512WithBadSignature()
    {
      Assert.Throws<SignatureOutOfRangeException>(() =>
      {
        SecretKeyAuth.VerifyHmacSha512(Encoding.UTF8.GetBytes("Adam Caudill"),
          Utilities.HexToBinary(
            "9f44681a662b7cde80c4eb34db5102b62a8b482272e3cceef73a334ec1d321c06a99b828e2ff921b4d1304bbd9480adfacf8c4c2ffbcbb4e5663446fda1235"),
          Encoding.UTF8.GetBytes("01234567890123456789012345678901"));
      });
    }
  }
}