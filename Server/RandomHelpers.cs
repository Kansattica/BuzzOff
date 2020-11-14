namespace BuzzOff.Server
{
	// Not super random, but close enough.
	public static class RandomHelpers
	{
		private static readonly ImmutableCircularBuffer<string> _adjectives = new ImmutableCircularBuffer<string>("Gay", "Illegal", "Criminal", "Unusual", "Vague", "Gruesome", "Marvelous", "Available", "Near", "Anxious", "Icy", "Glamorous", "Godly", "Great", "Furtive", "Late", "Neat", "Absorbed", "Clammy", "Frightening", "Exotic", "Wiggly", "Radical", "Thundering", "Colorful", "Stale", "Subdued", "Abrupt", "Ancient", "Smoggy", "Enthusiastic", "Good", "Fertile", "Damp", "Weary", "Many", "Chief", "Raspy", "Broad", "Heady", "Gentle", "Caring", "Gullible", "Lethal", "Royal", "Receptive", "Steep", "Defiant", "Fretful", "Spiteful", "Offbeat", "Bent", "Mushy", "Burly", "Bumpy", "Roasted", "Possible", "Halting", "Scandalous", "Gray", "Brief", "Futuristic", "Tangible", "Awake", "Puzzling", "Relieved", "Economic", "Frightened", "Thin", "Flaky", "Elegant", "Evanescent", "Impossible", "Embarrassed", "Functional", "Modern", "Judicious", "Gigantic", "Familiar", "Best", "Festive", "Common", "Tart", "Supreme", "Nonchalant", "Normal", "Wealthy", "Charming", "Cool", "Rad", "Double", "Triple", "Quadruple", "Space", "Skateboarding", "Friendly", "Big", "Small", "Huge", "Tiny", "Giant", "Microscopic");
		private static readonly ImmutableCircularBuffer<string> _nouns = new ImmutableCircularBuffer<string>("Skunk", "Rabbits", "Goldfish", "Stem", "Drink", "Chairs", "Finger", "Lunchroom", "Butter", "Income", "Exchange", "Show", "Event", "Top", "Oatmeal", "Neck", "Growth", "Comfort", "Goose", "Appliance", "Things", "Flower", "Birthday", "Marble", "Interest", "Crowd", "List", "Bears", "Jellyfish", "Kettle", "Letters", "Weight", "Fire", "Feet", "Library", "Distance", "Boot", "Fan", "Soda", "Chain", "Curve", "Tomatoes", "Punishment", "Opinion", "Daughter", "Skunks", "Support", "Thrill", "Dinosaurs", "Tendency", "Company", "Downtown", "Clock", "Airplane", "Mailbox", "Government", "Calendar", "Purpose", "Harmony", "Letter", "Cattle", "Office", "Wheel", "Cactus", "Girl", "Pain", "Thunder", "Snail", "Behavior", "Property", "Sweater", "Expert", "Sleep", "Mask", "Umbrella", "Attention", "Transport", "Quartz", "Spy", "Basin", "Fruit", "Noise", "Books", "Theory", "Bucket", "Spark", "Banana", "Dinner", "Hat", "Pants", "Shoe", "Airplanes", "Boat", "Space", "Time", "Crime", "Bug", "Bee", "Moth", "Friend", "Bird", "Robot", "Familiar", "Mouth");
		private static readonly ImmutableCircularBuffer<string> _emojis = new ImmutableCircularBuffer<string>("💜", "🦨", "✨", "💖", "🦹‍♀️", "🎀", "👸", "💽", "⚡", "🔥", "💎");

		public static string RandomUserName() => _adjectives.Next() + _nouns.Next();

		public static string RandomRoomName() => 
			string.Concat(_adjectives.Next(), _adjectives.Next(), _adjectives.Next(), _nouns.Next());

		public static string RandomEmoji() => _emojis.Next();

	}
}
