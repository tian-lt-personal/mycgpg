namespace MyCgpg;

using MyCgpg.Model;

public class NaiveWb
{
    public static IEnumerable<string> MoveWord(string text)
    {
        string word = string.Empty;
        foreach (char c in text)
        {
            if (c != ' ')
            {
                word += c;
            }
            else
            {
                if (word.Length > 0)
                {
                    yield return word;
                    word = string.Empty;
                }
                yield return " ";
            }
        }
        if (word.Length > 0)
        {
            yield return word;
        }
        yield break;
    }
}

public class GreedyLb
{
    public static IEnumerable<(Position<int>, string)> MoveCursor(
        IEnumerable<string> paragraph,
        int lineWidth,
        int lineHeight,
        Func<string, Extent<int>> getWordExtent)
    {
        var pos = new Position<int> { X = 0, Y = 0 };
        foreach (var word in paragraph)
        {
            var extent = getWordExtent(word);
            if (pos.X + extent.W <= lineWidth)
            {
                pos.X += extent.W;
                yield return (pos, word);
            }
            else
            {
                if (extent.W > lineWidth)
                {
                    throw new InsufficientLineWidthError();
                }
                pos.X = 0;
                pos.Y += lineHeight;
                yield return (pos, word);
            }
        }
        yield break;
    }
}