using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicDirDiff.Tests {
    [TestClass()]
    public class SizeCheckerTests {

        [TestMethod(), TestCategory("同期False")]
        public void Bak1_Org1_Last1() {
            //arrange
            long backupsize = 1, orgsizez = 1, lastsyncsize = 1;

            //act
            SizeChecker sc = new SizeChecker(backupsize, orgsizez, lastsyncsize);

            //assert
            Assert.AreEqual(expected: false, actual: sc.NeedtoUpdate);
        }

        [TestMethod(), TestCategory("同期False")]
        public void Bak2_Org1_Last1() {
            //arrange
            long backupsize = 2, orgsizez = 1, lastsyncsize = 1;

            //act
            SizeChecker sc = new SizeChecker(backupsize, orgsizez, lastsyncsize);

            //assert
            Assert.AreEqual(expected: false, actual: sc.NeedtoUpdate);
        }

        [TestMethod(), TestCategory("同期TRUE")]
        public void Bak1_Org1_Last0() {
            //arrange
            long backupsize = 1, orgsizez = 1, lastsyncsize = 0;

            //act
            SizeChecker sc = new SizeChecker(backupsize, orgsizez, lastsyncsize);

            //assert
            Assert.AreEqual(expected: true, actual: sc.NeedtoUpdate);
        }
    }
}