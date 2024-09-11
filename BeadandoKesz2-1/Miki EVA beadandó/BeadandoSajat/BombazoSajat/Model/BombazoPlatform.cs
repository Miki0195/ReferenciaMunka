using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Model
{
    public abstract class BombazoPlatform
    {
        public BombazoPlatform() { }

        public virtual bool IsRock()
        {
            return false; 
        }
    }
    public class Floor : BombazoPlatform 
    {
        public Floor() : base() { }
    }
    public class Rock : BombazoPlatform 
    {
        public Rock() : base() { }
        public override bool IsRock()
        {
            return true;
        }
    }
}
