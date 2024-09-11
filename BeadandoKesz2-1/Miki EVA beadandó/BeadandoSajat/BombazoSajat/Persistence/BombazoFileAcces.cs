using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Persistence
{
    public class BombazoFileAcces : IBombazoDataAcces
    {
        public BombazoFileAcces()
        {

        }
        public string[] Load(string resourceName)
        {
            try
            {
                return File.ReadAllLines(resourceName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}

