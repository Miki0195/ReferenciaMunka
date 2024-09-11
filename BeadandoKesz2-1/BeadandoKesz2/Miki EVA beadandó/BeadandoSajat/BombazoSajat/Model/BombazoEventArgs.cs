using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Model
{
    public class BombazoEventArgs : EventArgs
    {
        private bool _isWon;

        public bool IsWon {  get { return _isWon; } }
        
        public BombazoEventArgs(bool isWon) 
        {
            _isWon = isWon;
        }
    }
}
