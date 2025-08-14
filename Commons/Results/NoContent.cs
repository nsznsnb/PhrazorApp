namespace PhrazorApp.Commons.Results
{
    /// <summary>
    /// 戻り値不要の処理で、ジェネリックの「空」を表現するための型。
    /// C# の void は型引数に使えないため、その代替。
    /// </summary>
    public readonly struct NoContent : IEquatable<NoContent>
    {
        public static readonly NoContent Value = new();

        public bool Equals(NoContent other) => true;
        public override bool Equals(object? obj) => obj is NoContent;
        public override int GetHashCode() => 0;
        public override string ToString() => "Unit";

        public static bool operator ==(NoContent left, NoContent right) => true;
        public static bool operator !=(NoContent left, NoContent right) => false;
    }
}
