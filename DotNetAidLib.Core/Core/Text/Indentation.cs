using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Text
{
    public class Indentation
    {
        private readonly char indentChar;
        private readonly int indentCount;
        private int level;

        public Indentation(char indentChar, int indentCount, int level = 0)
        {
            this.indentChar = indentChar;
            this.indentCount = indentCount;
            Level = level;
        }

        public int Level
        {
            get => level;
            set => Set(value);
        }

        public Indentation Increment()
        {
            level++;
            return this;
        }

        public Indentation Decrement()
        {
            if (level > 0)
                level--;
            return this;
        }

        public Indentation Reset()
        {
            level = 0;
            return this;
        }

        public Indentation Set(int level)
        {
            this.level = Assert.GreaterOrEqualThan(level, 0, nameof(level));
            return this;
        }

        public static Indentation operator ++(Indentation v)
        {
            return v.Increment();
        }

        public static Indentation operator --(Indentation v)
        {
            return v.Decrement();
        }

        public Indentation Clone()
        {
            return new Indentation(indentChar, indentCount, level);
        }

        public override string ToString()
        {
            if (level > 0)
                return new string(indentChar, indentCount * level);
            return "";
        }
    }
}