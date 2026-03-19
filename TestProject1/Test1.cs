namespace TestProject1
{
    [TestClass]
    public class AccessControlTests
    {
        [TestMethod]
        public void CanManageEmployees_Admin_ReturnsTrue()
        {
            // Arrange
            int adminAccessLevel = 5;

            // Act
            bool result = AccessControl.CanManageEmployees(adminAccessLevel);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanManageEmployees_Waiter_ReturnsFalse()
        {
            // Arrange
            int waiterAccessLevel = 2;

            // Act
            bool result = AccessControl.CanManageEmployees(waiterAccessLevel);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
