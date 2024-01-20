namespace MyCgpg.Model;

public struct Position<T> where T : struct
{
    public T X, Y;
}

public struct Extent<T> where T : struct
{
    public T W, H;
}