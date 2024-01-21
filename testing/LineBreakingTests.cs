namespace MyCgpg.Testing;

using MyCgpg;
using MyCgpg.Model;
using System.Text;

public class LineBreakingTests
{
    [Test]
    public void DevDbg0()
    {
    }

    [TestCase("", 10, 8, ExpectedResult = "")]
    [TestCase(" ", 8, 8, ExpectedResult = " ")]
    [TestCase("  ", 8, 8, ExpectedResult = " \r\n ")]
    [TestCase(" ", 1, 8, ExpectedResult = null)]
    [TestCase("hello line breaking algo", 80, 8, ExpectedResult = "hello line\r\n breaking \r\nalgo")]
    [TestCase("hello line breaking algo", 100, 8, ExpectedResult = "hello line \r\nbreaking algo")]
    public string? FixedWith(string paragraph, int lineWidth, int glyphWidth)
    {
        try
        {
            return PrintPara(paragraph, lineWidth, glyphWidth);
        }
        catch (InsufficientLineWidthError)
        {
            return null;
        }
    }

    private string PrintPara(string text, int lineWidth, int glyphWidth)
    {
        var currY = 0.0;
        var sb = new StringBuilder();
        foreach (var (pos, word) in
            GreedyLb.MoveCursor(
                NaiveWb.MoveWord(text),
                lineWidth,
                20,
                word => GetWordExtent(word, glyphWidth)))
        {
            if (Math.Abs(currY - pos.Y) > 0.001)
            {
                currY = pos.Y;
                sb.AppendLine();
            }
            sb.Append(word);
        }
        return sb.ToString();
    }

    private static Extent<double> GetWordExtent(string word, int glyphWidth)
    {
        return new Extent<double> { W = glyphWidth * word.Length, H = 16 };
    }
}