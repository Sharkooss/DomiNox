namespace DomiNox.Dominoes
{
    public sealed class DominoDefinition
    {
        public string Id { get; }
        public int Left { get; }
        public int Right { get; }
        public int Sum => Left + Right;
        public bool IsDouble => Left == Right;

        public DominoDefinition(string id, int left, int right)
        {
            _ = new DominoValue(left);
            _ = new DominoValue(right);

            Id = id;
            Left = left;
            Right = right;
        }

        public override string ToString() => $"{Left}|{Right}";
    }
}
