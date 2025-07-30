using NUnit.Framework;
using LMBackend.RAG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.RAG.Tests
{
    [TestFixture()]
    public class DocumentSplitterTests
    {
        [Test()]
        public void GetLines_TestPdf()
        {
            // Arrange
            string pdfPath = @"D:\tenny_lu\Documents\各種說明\出勤記錄查詢.pdf";
            byte[] data = File.ReadAllBytes(pdfPath);

            // Act
            List<string> lines = DocumentSplitter.GetLines(pdfPath, data);

            // Assert
            Assert.That(lines.Count, Is.GreaterThan(0));
        }

        [Test()]
        public void GetLines_TestPdfError()
        {
            // Arrange
            string pdfPath = @"WTF.pdf";
            byte[] data = new byte[0];

            // Act
            List<string> lines = DocumentSplitter.GetLines(pdfPath, data);

            // Assert
            Assert.That(lines, Is.EqualTo(null));
        }

        [Test()]
        public void GetLines_TestTxt()
        {
            // Arrange
            string txtPath = @"Hello.txt";
            byte[] data = Encoding.UTF8.GetBytes("Hello world!");

            // Act
            List<string> lines = DocumentSplitter.GetLines(txtPath, data);

            // Assert
            Assert.That(lines.Count, Is.GreaterThan(0));
        }

        [Test()]
        public void GetLines_TestDocx()
        {
            // Arrange
            string pdfPath = @"D:\tenny_lu\Documents\使用vllm.docx";
            byte[] data = File.ReadAllBytes(pdfPath);

            // Act
            List<string> lines = DocumentSplitter.GetLines(pdfPath, data);

            // Assert
            Assert.That(lines.Count, Is.GreaterThan(0));
        }

        [Test()]
        public void GetLines_TestDocxError()
        {
            // Arrange
            string pdfPath = @"WTF.docx";
            byte[] data = new byte[0];

            // Act
            List<string> lines = DocumentSplitter.GetLines(pdfPath, data);

            // Assert
            Assert.That(lines, Is.EqualTo(null));
        }

        [Test()]
        public void GetLines_TestNotSupportException()
        {
            // Arrange

            // Act
            void TestFunc()
            {
                List<string> lines = DocumentSplitter.GetLines("file.bin", new byte[1]);
            }

            // Assert
            Assert.Throws<NotSupportedException>(TestFunc);
        }

        [Test()]
        public void SplitText_Test()
        {
            // Arrange
            List<string> lines = new List<string>
            {
                "Hello",
                "World",
                "333.",
                ""
            };

            // Act
            List<string> linesOut = DocumentSplitter.SplitText(lines);

            // Assert
            Assert.That(lines.Count, Is.GreaterThan(0));
        }

        [Test()]
        public void SplitTextByWords_Test()
        {
            // Arrange
            List<string> lines = new List<string>
            {
                "Hello",
                "World",
                "333.",
                ""
            };

            // Act
            List<string> linesOut = DocumentSplitter.SplitTextByWords(lines);

            // Assert
            Assert.That(lines.Count, Is.GreaterThan(0));
        }
    }
}