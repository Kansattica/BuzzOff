namespace BuzzOff.Server
{
	// Not super random, but close enough.
	public static class RandomHelpers
	{
		private static readonly ImmutableCircularBuffer<string> _adjectives = new ImmutableCircularBuffer<string>("Gay", "Illegal", "Criminal", "Unusual", "Vague", "Gruesome", "Marvelous", "Available", "Near", "Anxious", "Icy", "Glamorous", "Godly", "Great", "Furtive", "Late", "Neat", "Absorbed", "Clammy", "Frightening", "Wiggly", "Radical", "Thundering", "Colorful", "Stale", "Subdued", "Abrupt", "Ancient", "Smoggy", "Enthusiastic", "Good", "Fertile", "Damp", "Weary", "Many", "Raspy", "Broad", "Heady", "Gentle", "Caring", "Gullible", "Royal", "Receptive", "Steep", "Defiant", "Fretful", "Spiteful", "Offbeat", "Bent", "Mushy", "Burly", "Bumpy", "Roasted", "Possible", "Halting", "Scandalous", "Gray", "Brief", "Futuristic", "Tangible", "Awake", "Puzzling", "Relieved", "Economic", "Frightened", "Thin", "Flaky", "Elegant", "Evanescent", "Impossible", "Embarrassed", "Functional", "Modern", "Judicious", "Gigantic", "Familiar", "Best", "Festive", "Common", "Tart", "Supreme", "Nonchalant", "Normal", "Wealthy", "Charming", "Cool", "Rad", "Double", "Triple", "Quadruple", "Space", "Skateboarding", "Friendly", "Big", "Small", "Huge", "Tiny", "Giant", "Microscopic", "Hovering", "Menacing", "Mysterious", "Majestic", "Magic");
		private static readonly ImmutableCircularBuffer<string> _nouns = new ImmutableCircularBuffer<string>("Skunk", "Rabbits", "Goldfish", "Stem", "Drink", "Chair", "Finger", "Lunchroom", "Butter", "Income", "Exchange", "Show", "Event", "Top", "Oatmeal", "Neck", "Growth", "Comfort", "Goose", "Appliance", "Thing", "Flower", "Birthday", "Marble", "Interest", "Crowd", "List", "Bear", "Jellyfish", "Kettle", "Letter", "Weight", "Fire", "Feet", "Library", "Distance", "Boot", "Fan", "Soda", "Chain", "Curve", "Tomato", "Punishment", "Opinion", "Daughter", "Support", "Thrill", "Dinosaur", "Tendency", "Company", "Downtown", "Clock", "Airplane", "Mailbox", "Government", "Calendar", "Purpose", "Harmony", "Letter", "Cattle", "Office", "Wheel", "Cactus", "Girl", "Pain", "Thunder", "Snail", "Behavior", "Property", "Sweater", "Expert", "Sleep", "Mask", "Umbrella", "Attention", "Transport", "Quartz", "Spy", "Basin", "Fruit", "Noise", "Book", "Theory", "Bucket", "Spark", "Banana", "Dinner", "Hat", "Pants", "Shoe", "Airplane", "Boat", "Space", "Time", "Crime", "Bug", "Bee", "Moth", "Friend", "Bird", "Robot", "Familiar", "Mouth", "Fox", "Box", "Lock", "Skeleton", "Giraffe", "Owl", "Vampire", "Werewolf", "Scientist", "Dog", "Cat", "Shark", "Computer", "Virus", "Train");
		private static readonly ImmutableCircularBuffer<string> _emojis = new ImmutableCircularBuffer<string>("💜", "🦨", "✨", "💖", "🦹‍♀️", "🎀", "👸", "💽", "⚡", "🔥", "💎");

		public static string RandomUserName() => _adjectives.Next() + _nouns.Next();

		public static string RandomRoomName() => 
			string.Concat(_adjectives.Next(), _adjectives.Next(), _adjectives.Next(), _nouns.Next());

		public static string RandomEmoji() => _emojis.Next();

	}
}
