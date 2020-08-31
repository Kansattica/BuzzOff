using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzOff.Shared
{
	public static class RandomName
	{
		private static readonly Random _rand = new Random();

		private static readonly string[] _adjectives = new[] { "Vague", "Gruesome", "Marvelous", "Available", "Near", "Anxious", "Icy", "Glamorous", "Godly", "Great", "Furtive", "Late", "Neat", "Absorbed", "Clammy", "Frightening", "Exotic", "Wiggly", "Thundering", "Colorful", "Stale", "Subdued", "Abrupt", "Ancient", "Smoggy", "Enthusiastic", "Good", "Normal", "Fertile", "Damp", "Weary", "Many", "Chief", "Raspy", "Broad", "Heady", "Gentle", "Caring", "Gullible", "Lethal", "Royal", "Receptive", "Steep", "Defiant", "Fretful", "Spiteful", "Offbeat", "Bent", "Mushy", "Burly", "Bumpy", "Roasted", "Possible", "Halting", "Scandalous", "Gray", "Brief", "Futuristic", "Tangible", "Awake", "Puzzling", "Relieved", "Economic", "Skinny", "Frightened", "Thin", "Flaky", "Elegant", "Evanescent", "Impossible", "Embarrassed", "Functional", "Modern", "Judicious", "Gigantic", "Familiar", "Best", "Festive", "Common", "Tart", "Supreme", "Nonchalant", "Normal", "Wealthy", "Furtive", "Charming", "Cool" };
		private static readonly string[] _nouns = new[] { "Rabbits", "Goldfish", "Stem", "Drink", "Chairs", "Finger", "Lunchroom", "Butter", "Income", "Exchange", "Show", "Event", "Top", "Oatmeal", "Neck", "Growth", "Comfort", "Goose", "Appliance", "Things", "Flower", "Birthday", "Marble", "Interest", "Crowd", "List", "Bears", "Jellyfish", "Kettle", "Letters", "Weight", "Fire", "Feet", "Library", "Distance", "Boot", "Fan", "Soda", "Chain", "Curve", "Tomatoes", "Punishment", "Opinion", "Daughter", "Support", "Thrill", "Dinosaurs", "Tendency", "Company", "Downtown", "Clock", "Airplane", "Mailbox", "Government", "Calendar", "Purpose", "Harmony", "Letter", "Cattle", "Office", "Airplane", "Wheel", "Cactus", "Girl", "Pain", "Thunder", "Snail", "Behavior", "Property", "Sweater", "Expert", "Sleep", "Mask", "Umbrella", "Attention", "Transport", "Quartz", "Spy", "Basin", "Fruit", "Noise", "Books", "Theory", "Bucket", "Spark", "Banana", "Dinner" };

		public static string RandomString(int adjectiveCount)
		{
			return string.Join(string.Empty, Enumerable.Range(0, adjectiveCount).Select(_ => Sample(_adjectives))) 
				+ Sample(_nouns);
		}

		private static string Sample(string[] array)
		{
			lock (_rand)
			{
				return array[_rand.Next(0, array.Length)];
			}
		}

	}
}
