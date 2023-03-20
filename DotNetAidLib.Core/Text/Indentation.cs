using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Text
{
    public class Indentation{
        private char indentChar;
        private int indentCount;
        private int level = 0;

        public Indentation(char indentChar, int indentCount, int level=0){
            this.indentChar = indentChar;
            this.indentCount = indentCount;
            this.Level = level;
        }

        public int Level
        {
            get { return this.level; }
            set { this.Set(value); }
        }

        public Indentation Increment() {
            this.level++;
            return this;
        }

        public Indentation Decrement()
        {
            if(this.level>0)
                this.level--;
            return this;
        }

        public Indentation Reset(){
            this.level=0;
            return this;
        }

        public Indentation Set(int level)
        {
            this.level = Assert.GreaterOrEqualThan(level, 0, nameof(level));
            return this;
        }

        public static Indentation operator++(Indentation v) {
            return v.Increment();
        }

        public static Indentation operator--(Indentation v){
            return v.Decrement();
        }

        public Indentation Clone() {
            return new Indentation(this.indentChar, this.indentCount, this.level);
        }

        public override string ToString()
        {
            if (this.level > 0)
                return new string(this.indentChar, (this.indentCount * this.level));
            else
                return "";
        }
    }
}
