using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Principal interface for working with calculators
    ///             content (i.e. uris). 
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///References:	
    /// </summary>
    public interface ICalculator1
    {
        //get the calculators
        
        IEnumerable<Calculator1> GetCalculators(Calculator1 calculator);
    }
}
