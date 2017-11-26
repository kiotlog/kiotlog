﻿using System.Text;
using NUnit.Framework;
using Sodium;

namespace Tests
{
  /// <summary>Tests for the PublicKeyAuth class</summary>
  [TestFixture]
  public class PublicKeyAuthTest
  {
    /// <summary>Does PublicKeyAuth.GenerateKeyPair() return... something.</summary>
    [Test]
    public void GenerateKeyTest()
    {
      var actual = PublicKeyAuth.GenerateKeyPair();

      //need a better test
      Assert.IsNotNull(actual.PrivateKey);
      Assert.IsNotNull(actual.PublicKey);
    }

    /// <summary>Does PublicKeyAuth.GenerateKeyPair(seed) return the expected value?</summary>
    [Test]
    public void GenerateKeySeedTest()
    {
      var expected = new KeyPair(Utilities.HexToBinary("76a1592044a6e4f511265bca73a604d90b0529d1df602be30a19a9257660d1f5"),
        Utilities.HexToBinary("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff76a1592044a6e4f511265bca73a604d90b0529d1df602be30a19a9257660d1f5"));
      var actual = PublicKeyAuth.GenerateKeyPair(Utilities.HexToBinary("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));

      CollectionAssert.AreEqual(expected.PublicKey, actual.PublicKey);
      CollectionAssert.AreEqual(expected.PrivateKey, actual.PrivateKey);
    }
    
    /// <summary>Does PublicKeyAuth.Sign() return the expected value?</summary>
    [Test]
    public void SimpleAuthTest()
    {
      var expected = Utilities.HexToBinary("8d5436accbe258a6b252c1140f38d7b8dc6196619945818b72512b6a8019d86dfeeb56f40c4d4b983d97dfeed37948527256c3567d6b253757fcfb32bef56f0b4164616d2043617564696c6c");
      var actual = PublicKeyAuth.Sign(Encoding.UTF8.GetBytes("Adam Caudill"),
        Utilities.HexToBinary("89dff97c131434c11809c3341510ce63c85e851d3ba62e2f810016bbc67d35144ffda13c11d61d2b9568e54bec06ea59368e84874883087645e64e5e9653422e"));
      CollectionAssert.AreEqual(expected, actual);
    }

    /// <summary>Does SecretKeyAuth.Verify() return the expected value?</summary>
    [Test]
    public void SimpleVerifyTest()
    {
      var expected = Encoding.UTF8.GetBytes("Adam Caudill");
      var actual = PublicKeyAuth.Verify(Utilities.HexToBinary("8d5436accbe258a6b252c1140f38d7b8dc6196619945818b72512b6a8019d86dfeeb56f40c4d4b983d97dfeed37948527256c3567d6b253757fcfb32bef56f0b4164616d2043617564696c6c"),
        Utilities.HexToBinary("4ffda13c11d61d2b9568e54bec06ea59368e84874883087645e64e5e9653422e"));
      CollectionAssert.AreEqual(expected, actual);
    }

    /// <summary>Does PublicKeyAuth.SignDetached() return the expected value?</summary>
    [Test]
    public void SimpleAuthDetachedTest()
    {
      var expected = Utilities.HexToBinary("8d5436accbe258a6b252c1140f38d7b8dc6196619945818b72512b6a8019d86dfeeb56f40c4d4b983d97dfeed37948527256c3567d6b253757fcfb32bef56f0b");
      var actual = PublicKeyAuth.SignDetached(Encoding.UTF8.GetBytes("Adam Caudill"),
        Utilities.HexToBinary("89dff97c131434c11809c3341510ce63c85e851d3ba62e2f810016bbc67d35144ffda13c11d61d2b9568e54bec06ea59368e84874883087645e64e5e9653422e"));
      CollectionAssert.AreEqual(expected, actual);
    }

    /// <summary>Does SecretKeyAuth.VerifyDetached() return the expected value?</summary>
    [Test]
    public void SimpleVerifyDetachedTest()
    {
      var actual = PublicKeyAuth.VerifyDetached(
        Utilities.HexToBinary("8d5436accbe258a6b252c1140f38d7b8dc6196619945818b72512b6a8019d86dfeeb56f40c4d4b983d97dfeed37948527256c3567d6b253757fcfb32bef56f0b"),
        Encoding.UTF8.GetBytes("Adam Caudill"), 
        Utilities.HexToBinary("4ffda13c11d61d2b9568e54bec06ea59368e84874883087645e64e5e9653422e"));
       
      Assert.IsTrue(actual);
    }

    [Test]
    public void ExtractEd25519SeedFromEd25519SecretKeyTest()
    {
      // generate an Ed25519 keypair
      var firstKeypair = PublicKeyAuth.GenerateKeyPair();
      // extract the seed from the generated keypair
      var seed = PublicKeyAuth.ExtractEd25519SeedFromEd25519SecretKey(firstKeypair.PrivateKey);
      // generate a second keypair from the seed
      var secondKeyPair = PublicKeyAuth.GenerateKeyPair(seed);
      CollectionAssert.AreEqual(firstKeypair.PublicKey, secondKeyPair.PublicKey);
      CollectionAssert.AreEqual(firstKeypair.PrivateKey, secondKeyPair.PrivateKey);
    }

    [Test]
    public void ExtractEd25519PublicKeyFromEd25519SecretKey()
    {
      // generate an Ed25519 keypair
      var keypair = PublicKeyAuth.GenerateKeyPair();
      // extract the seed from the generated keypair
      var publicKey = PublicKeyAuth.ExtractEd25519PublicKeyFromEd25519SecretKey(keypair.PrivateKey);
      CollectionAssert.AreEqual(keypair.PublicKey, publicKey);
    }

    [Test]
    public void PublicKeyAuthConvertToCurve25519()
    {
      var keypairSeed = new byte[]{
        0x42, 0x11, 0x51, 0xa4, 0x59, 0xfa, 0xea, 0xde,
        0x3d, 0x24, 0x71, 0x15, 0xf9, 0x4a, 0xed, 0xae,
        0x42, 0x31, 0x81, 0x24, 0x09, 0x5a, 0xfa, 0xbe,
        0x4d, 0x14, 0x51, 0xa5, 0x59, 0xfa, 0xed, 0xee
      };

      var keys = PublicKeyAuth.GenerateKeyPair(keypairSeed);

      var ed25519Pk = keys.PublicKey;
      var ed25519SkPk = keys.PrivateKey;

      var curve25519Pk = PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey(ed25519Pk);
      var curve25519Sk = PublicKeyAuth.ConvertEd25519SecretKeyToCurve25519SecretKey(ed25519SkPk);

      Assert.AreEqual(Utilities.BinaryToHex(curve25519Pk, Utilities.HexFormat.None, Utilities.HexCase.Upper),
                      "F1814F0E8FF1043D8A44D25BABFF3CEDCAE6C22C3EDAA48F857AE70DE2BAAE50");
      Assert.AreEqual(Utilities.BinaryToHex(curve25519Sk, Utilities.HexFormat.None, Utilities.HexCase.Upper),
                      "8052030376D47112BE7F73ED7A019293DD12AD910B654455798B4667D73DE166");

      for(var i = 0; i < 500; i++)
      {
        keys = PublicKeyAuth.GenerateKeyPair();
        ed25519Pk = keys.PublicKey;
        ed25519SkPk = keys.PrivateKey;
        curve25519Pk = PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey(ed25519Pk);
        curve25519Sk = PublicKeyAuth.ConvertEd25519SecretKeyToCurve25519SecretKey(ed25519SkPk);
        var curve25519Pk2 = ScalarMult.Base(curve25519Sk);

        CollectionAssert.AreEqual(curve25519Pk, curve25519Pk2);
      }
    }
  }
}
