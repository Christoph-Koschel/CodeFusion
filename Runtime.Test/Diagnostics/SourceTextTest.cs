using IllusionScript.Runtime.Diagnostics;
using Xunit;

namespace IllusionScript.Runtime.Test.Diagnostics;

public class SourceTextTest
{
    public class SourceTextTests
    {
        [Theory]
        [InlineData(".", 1)]
        [InlineData(".\r\n", 2)]
        [InlineData(".\r\n\r\n", 3)]
        public void SourceTextIncludesLastLine(string text, int expectedLineCount)
        {
            SourceText sourceText = SourceText.From(text);
            Assert.Equal(expectedLineCount, sourceText.lines.Length);
        }
    }
}