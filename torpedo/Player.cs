namespace NationalInstruments
{
    public class Player
    {
        public string Name { get; init; }

        public Player(string name)
        {
            Name = name;
        }

        public override string? ToString()
        {
            return Name;
        }
    }
}