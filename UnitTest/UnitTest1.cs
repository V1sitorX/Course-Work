namespace UnitTest;
[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void EncryptionTest()
    {
        DoublePermutation Test = new DoublePermutation();
        string TestEncryptText = "Encyption test";
        int[] TestcolumnOrder = { 1, 0, 2 };
        int[] TestrowOrder = { 0, 1, 2 };
        string encryptedTest = Test.Encrypt(TestEncryptText, TestcolumnOrder, TestrowOrder);
        Assert.AreNotEqual(TestEncryptText, encryptedTest, "Final text doesnt match original");
    }
    [TestMethod]
    public void DecryptionTest()
    {
        DoublePermutation Test = new DoublePermutation();
        string TestDecryptText = "Decryption test";
        int[] TestcolumnOrder = { 1, 0, 2 };
        int[] TestrowOrder = { 0, 1, 2 };
        string decryptedTest = Test.Encrypt(TestDecryptText, TestcolumnOrder, TestrowOrder);
        Assert.AreNotEqual(TestDecryptText, decryptedTest, "Final text doesnt match original");
    }
    [TestMethod]
    public void FullProcessTest()
    {
        DoublePermutation Test = new DoublePermutation();
        string TestText = "This text was created for testing encryption and decryption process";
        int[] TestcolumnOrder = { 1, 0, 2 };
        int[] TestrowOrder = { 0, 1, 2 };
        string encryptedTest = Test.Encrypt(TestText, TestcolumnOrder, TestrowOrder);
        string decryptedTest = Test.Decrypt(encryptedTest,TestcolumnOrder, TestrowOrder);
        Assert.AreEqual(TestText, decryptedTest, "Final text doesnt match a first text");
    }
}