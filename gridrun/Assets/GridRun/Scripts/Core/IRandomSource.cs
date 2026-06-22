using System;

namespace GridRun.Core
{
    public interface IRandomSource
    {
        int Range(int minInclusive, int maxExclusive);
    }

    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource()
            : this(Environment.TickCount)
        {
        }

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }

    public sealed class DeterministicRandomSource : IRandomSource
    {
        private readonly Random _random;

        public DeterministicRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
