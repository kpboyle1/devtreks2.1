﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		This class extends the base BudgetInvestmentGroup with 
    ///             labor scheduling and timeliness penalty parameters. These 
    ///             parameters are used in calculators and resource stock analyzers 
    ///             to schedule operations and to penalize late operations and 
    ///             optimize machinery selections.
    ///Author:		www.devtreks.org
    ///Date:		2012, February
    ///References:	www.devtreks.org/helptreks/linkedviews/help/linkedview/HelpFile/148
    public class TimelinessBIGroup1 : BudgetInvestmentGroup
    {
        public TimelinessBIGroup1() { }
        //scheduling and selection optimization collection
        //each member is a feasible combination of power and nopower inputs for that opcomp
        public List<TimelinessBI1> TimelinessBudgets { get; set; }
    }
}
