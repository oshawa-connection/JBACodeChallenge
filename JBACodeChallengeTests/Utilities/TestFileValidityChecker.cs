using TestsJBACodeInterview;
using TestsJBACodeInterview.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JBACodeInterview.Utilities;
using System.IO;
using System;

namespace TestsJBACodeInterview.Utilities
{
    [TestClass]
    public class TestFileValidityChecker
    {
        string wrongFilePathCorrectDirectory;
        string wrongFilePath;
        string wrongFilePathWrongFileExtension;
        string correctFilePath;

        public TestFileValidityChecker()
        {
            wrongFilePathCorrectDirectory = @"..\files\nonExistantFile.pre";
            wrongFilePath = @"..\nonExistantDirectory\nonExistantFile.pre";
            wrongFilePathWrongFileExtension = @"..\files\JBATestFile.jba"; 
            correctFilePath = @"..\files\JBATestFile.pre";
        }


        [TestMethod]
        public void testCheckPreFileValidity()
        {
            FileValidityChecker fileValidityChecker = new FileValidityChecker();

            Assert.ThrowsException<Exception>(() => fileValidityChecker.checkPreFileValidity(wrongFilePathCorrectDirectory),
                    "Should throw exception if directory exists but no pre file exists there.");


            Assert.ThrowsException<Exception>(() => fileValidityChecker.checkPreFileValidity(wrongFilePathCorrectDirectory),
                "Should throw exception if directory exists but no pre file exists there.");

        }
    }
}
