namespace TestProject1;

[TestClass]
public class PasswordHelperTests
{
    [TestMethod]
    public void GenerateHash_ValidPassword_ReturnsHashAndSalt()
    {
        // Arrange
        string password = "TestPassword123";

        // Act
        var (hash, salt) = PasswordHelper.GenerateHash(password);

        // Assert
        Assert.IsNotNull(hash);
        Assert.IsNotNull(salt);
        Assert.IsTrue(hash.Length > 0);
        Assert.IsTrue(salt.Length > 0);
    }

    [TestMethod]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        string password = "TestPassword123";
        var (hash, salt) = PasswordHelper.GenerateHash(password);

        // Act
        bool result = PasswordHelper.VerifyPassword(password, hash, salt);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        string originalPassword = "TestPassword123";
        string wrongPassword = "WrongPassword456";
        var (hash, salt) = PasswordHelper.GenerateHash(originalPassword);

        // Act
        bool result = PasswordHelper.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.IsFalse(result);
    }
}
