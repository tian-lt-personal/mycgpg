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

    [TestCase("hello line breaking algo", 80, ExpectedResult = "hello \r\nline breaking\r\n algo")]
    [TestCase("hello line breaking algo", 100, ExpectedResult = "hello line \r\nbreaking algo")]
    public string FixedWith(string paragraph, int lineWidth)
    {
        return PrintPara(paragraph, lineWidth);
    }

    private string PrintPara(string text, int lineWidth)
    {
        var currY = 0;
        var sb = new StringBuilder();
        foreach (var (pos, word) in GreedyLb.MoveCursor(NaiveWb.MoveWord(text), lineWidth, 20, GetWordExtent))
        {
            if (currY != pos.Y)
            {
                currY = pos.Y;
                sb.AppendLine();
            }
            sb.Append(word);
        }
        return sb.ToString();
    }

    private static Extent<int> GetWordExtent(string text)
    {
        return new Extent<int> { W = 8 * text.Length, H = 16 };
    }
}