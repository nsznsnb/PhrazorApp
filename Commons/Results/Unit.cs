namespace PhrazorApp.Commons.Results
{
    /// <summary>
    /// 戻り値不要の処理で、ジェネリックの「空」を表現するための型。
    /// C# の void は型引数に使えないため、その代替。
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
