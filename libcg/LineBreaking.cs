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
    public static IEnumerable<(Position<double>, string)> MoveCursor(
        IEnumerable<string> paragraph,
        double lineWidth,
        double lineHeight,
        Func<string, Extent<double>> getWordExtent)
    {
        var pos = new Position<double> { X = 0.0, Y = 0.0 };
        foreach (var word in paragraph)
        {
            var extent = getWordExtent(word);
            if (pos.X + extent.W <= lineWidth)
            {
                var retval = pos;
                pos.X += extent.W;
                yield return (retval, word);
            }
            else
            {
                if (extent.W > lineWidth)
                {
                    throw new InsufficientLineWidthError();
                }
                pos.X = 0.0;
                pos.Y += lineHeight;
                yield return (pos, word);
            }
        }
        yield break;
    }
}