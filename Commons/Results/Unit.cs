namespace PhrazorApp.Commons.Results
{
    /// <summary>
    /// 「値がない」ことを表す軽量な型（F# / Scala の Unit に相当）。<br />
    /// - C# の void は型引数に使えないため、ジェネリック戻り値の「空」を表すために用意。
    /// </summary>
    public readonly struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Value = new();

        public bool Equals(Unit other) => true;
        public override bool Equals(object? obj) => obj is Unit;
        public override int GetHashCode() => 0;
        public override string ToString() => "Unit";

        public static bool operator ==(Unit left, Unit right) => true;
        public static bool operator !=(Unit left, Unit right) => false;
    }
}
