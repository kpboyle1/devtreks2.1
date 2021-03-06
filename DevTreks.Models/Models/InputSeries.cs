﻿using System;
using System.Collections.Generic;

namespace DevTreks.Models
{
    public partial class InputSeries
    {
        public InputSeries()
        {
            BudgetSystemToInput = new HashSet<BudgetSystemToInput>();
            ComponentToInput = new HashSet<ComponentToInput>();
            CostSystemToInput = new HashSet<CostSystemToInput>();
            LinkedViewToInputSeries = new HashSet<LinkedViewToInputSeries>();
            OperationToInput = new HashSet<OperationToInput>();
        }

        public int PKId { get; set; }
        public string Num { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime InputDate { get; set; }
        public string InputUnit1 { get; set; }
        public decimal InputPrice1 { get; set; }
        public float InputPrice1Amount { get; set; }
        public string InputUnit2 { get; set; }
        public decimal InputPrice2 { get; set; }
        public string InputUnit3 { get; set; }
        public decimal InputPrice3 { get; set; }
        public DateTime InputLastChangedDate { get; set; }
        public int RealRateId { get; set; }
        public int NominalRateId { get; set; }
        public int GeoCodeId { get; set; }
        public int DataSourceId { get; set; }
        public int CurrencyClassId { get; set; }
        public int UnitClassId { get; set; }
        public int RatingClassId { get; set; }
        public int InputId { get; set; }

        public virtual ICollection<BudgetSystemToInput> BudgetSystemToInput { get; set; }
        public virtual ICollection<ComponentToInput> ComponentToInput { get; set; }
        public virtual ICollection<CostSystemToInput> CostSystemToInput { get; set; }
        public virtual ICollection<LinkedViewToInputSeries> LinkedViewToInputSeries { get; set; }
        public virtual ICollection<OperationToInput> OperationToInput { get; set; }
        public virtual Input Input { get; set; }
    }
}
