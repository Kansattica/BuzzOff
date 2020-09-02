using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Routing.Tree;

namespace BuzzOff.Server
{
	// Not super random, but close enough.
	public static class RandomHelpers
	{
		private static readonly SyncSamplingArray<string> _adjectives = new SyncSamplingArray<string>("Gay", "Illegal", "Criminal", "Unusual", "Vague", "Gruesome", "Marvelous", "Available", "Near", "Anxious", "Icy", "Glamorous", "Godly", "Great", "Furtive", "Late", "Neat", "Absorbed", "Clammy", "Frightening", "Exotic", "Wiggly", "Thundering", "Colorful", "Stale", "Subdued", "Abrupt", "Ancient", "Smoggy", "Enthusiastic", "Good", "Fertile", "Damp", "Weary", "Many", "Chief", "Raspy", "Broad", "Heady", "Gentle", "Caring", "Gullible", "Lethal", "Royal", "Receptive", "Steep", "Defiant", "Fretful", "Spiteful", "Offbeat", "Bent", "Mushy", "Burly", "Bumpy", "Roasted", "Possible", "Halting", "Scandalous", "Gray", "Brief", "Futuristic", "Tangible", "Awake", "Puzzling", "Relieved", "Economic", "Frightened", "Thin", "Flaky", "Elegant", "Evanescent", "Impossible", "Embarrassed", "Functional", "Modern", "Judicious", "Gigantic", "Familiar", "Best", "Festive", "Common", "Tart", "Supreme", "Nonchalant", "Normal", "Wealthy", "Charming", "Cool", "Rad", "Radical", "Double", "Triple", "Quadruple", "Space");
		private static readonly SyncSamplingArray<string> _nouns = new SyncSamplingArray<string>("Skunk", "Skunks", "Rabbits", "Goldfish", "Stem", "Drink", "Chairs", "Finger", "Lunchroom", "Butter", "Income", "Exchange", "Show", "Event", "Top", "Oatmeal", "Neck", "Growth", "Comfort", "Goose", "Appliance", "Things", "Flower", "Birthday", "Marble", "Interest", "Crowd", "List", "Bears", "Jellyfish", "Kettle", "Letters", "Weight", "Fire", "Feet", "Library", "Distance", "Boot", "Fan", "Soda", "Chain", "Curve", "Tomatoes", "Punishment", "Opinion", "Daughter", "Support", "Thrill", "Dinosaurs", "Tendency", "Company", "Downtown", "Clock", "Airplane", "Mailbox", "Government", "Calendar", "Purpose", "Harmony", "Letter", "Cattle", "Office", "Wheel", "Cactus", "Girl", "Pain", "Thunder", "Snail", "Behavior", "Property", "Sweater", "Expert", "Sleep", "Mask", "Umbrella", "Attention", "Transport", "Quartz", "Spy", "Basin", "Fruit", "Noise", "Books", "Theory", "Bucket", "Spark", "Banana", "Dinner", "Hat", "Pants", "Shoe", "Airplanes", "Boat", "Space", "Time");
		private static readonly SyncSamplingArray<string> _emojis = new SyncSamplingArray<string>("💜", "🦨", "✨", "💖", "🦹‍♀️", "🎀", "👸", "💽", "⚡", "🔥");

        public static string RandomUserName() => _adjectives.Next() + _nouns.Next();

        public static string RandomRoomName() => 
			string.Concat(_adjectives.Next(), _adjectives.Next(), _adjectives.Next(), _nouns.Next());

        public static string RandomEmoji() => _emojis.Next();

		private class SyncSamplingArray<T>
		{
			private static readonly Random _rand = new Random();
			private readonly ImmutableArray<T> _data;
			private int idx;

            public SyncSamplingArray(params T[] Data)
            {
                _data = ImmutableArray.Create(Data);

				// without this, the server always starts on the same thing, which will lead to problems if the server restarts while
				// people are using it.
				lock(_rand)
                {
					idx = _rand.Next(0, _data.Length);
                }
            }

            public T Next()
			{
				var localIdx = Interlocked.Increment(ref idx);

				if (localIdx >= _data.Length)
				{
					var inBoundsIdx = localIdx % _data.Length;
					Interlocked.CompareExchange(ref idx, inBoundsIdx, localIdx);
					localIdx = inBoundsIdx;
				}
				return _data[localIdx];
			}
		}

	}
}
