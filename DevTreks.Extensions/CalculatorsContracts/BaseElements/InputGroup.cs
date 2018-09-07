﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

using DataHelpers = DevTreks.Data.Helpers;
using DataAppHelpers = DevTreks.Data.AppHelpers;
using DataEditHelpers = DevTreks.Data.EditHelpers;
using RuleHelpers = DevTreks.Data.RuleHelpers;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		The inputgroup class is a base class used by many
    ///             input calculators.
    ///Author:		www.devtreks.org
    ///Date:		2013, November
    ///References:	www.devtreks.org/helptreks/linkedviews/help/linkedview/HelpFile/148
    ///</summary>
    public class InputGroup : CostBenefitCalculator
    {
        public InputGroup() 
        {
            Init();
        }
        //properties
        public string DocStatus { get; set; }
        public int ServiceId { get; set; }
        public List<Input> Inputs { get; set; }
        //linked calculators/analyzers
        public List<Calculator1> Calculators { get; set; }
        public XElement XmlDocElement { get; set; }
        //locals (for whole xmldoc updates)
        public Local Local { get; set; }
        public void Init()
        {
            this.InitSharedObjectProperties();
            //init anything that will cause an exception when called
            this.Local = new Local();
            this.Inputs = new List<Input>();
            this.Calculators = new List<Calculator1>();
            this.DocStatus = string.Empty;
            this.ServiceId = 0;
        }
        //set the class properties using the XElement
        public void SetInputGroupProperties(
            CalculatorParameters calcParameters,
            XElement currentCalculationsElement, XElement currentElement)
        {
            //several extensions store some calculator props in base element (observations, targettype)
            //no harm done in setting them but never set their attributes in base element
            this.SetCalculatorProperties(currentCalculationsElement);
            this.SetSharedObjectProperties(currentElement);
            this.SetTotalCostsProperties(currentElement);
            this.DocStatus = CalculatorHelpers.GetAttribute(currentElement,
                DataAppHelpers.General.DOC_STATUS);
            this.ServiceId = CalculatorHelpers.GetAttributeInt(currentElement,
                DataAppHelpers.Agreement.SERVICE_ID);
            this.Local = new Local();
            //use the calculator params to set locals (they can be changed in calcor)
            this.Local.SetLocalProperties(calcParameters,
                currentCalculationsElement, currentElement);
            this.XmlDocElement = currentCalculationsElement;
        }
        //copy constructor
        public InputGroup(CalculatorParameters calcParameters, InputGroup inputGroup)
        {
            //several extensions store some calculator props in base element (observations, targettype)
            //no harm done in setting them but never set their attributes
            this.CopyCalculatorProperties(inputGroup);
            this.CopySharedObjectProperties(inputGroup);
            this.DocStatus = inputGroup.DocStatus;
            this.ServiceId = inputGroup.ServiceId;
            this.Type = inputGroup.Type;
            //better to set in base 
            this.CopyTotalCostsProperties(inputGroup);
            //calculators are always app-specific and must be copied subsequently
            this.Calculators = new List<Calculator1>();
            this.ErrorMessage = inputGroup.ErrorMessage;
            if (inputGroup.Local == null)
                inputGroup.Local = new Local();
            this.Local = new Local(calcParameters, inputGroup.Local);
            if (inputGroup.XmlDocElement != null)
            {
                this.XmlDocElement = new XElement(inputGroup.XmlDocElement);
            }
        }
        
        public void SetInputGroupAttributes(
            CalculatorParameters calcParameters, XElement currentElement,
            IDictionary<string, string> updates)
        {
            //version 1.4.5 requires setting calculator atts separately (so specific calc can be used)
            //serialize the current element 
            this.SetSharedObjectAttributes(string.Empty, currentElement);
            this.SetTotalCostsAttributes(string.Empty, currentElement);
            DataHelpers.AddInHelperLinq.UpdateNewValue(calcParameters.AttributeNeedsDbUpdate,
               calcParameters.CurrentElementURIPattern, DataAppHelpers.General.DOC_STATUS,
               this.DocStatus, RuleHelpers.GeneralRules.STRING,
               calcParameters.StepNumber, currentElement, updates);
            DataHelpers.AddInHelperLinq.UpdateNewValue(calcParameters.AttributeNeedsDbUpdate,
               calcParameters.CurrentElementURIPattern, DataAppHelpers.Agreement.SERVICE_ID,
               this.ServiceId.ToString(), RuleHelpers.GeneralRules.INTEGER,
               calcParameters.StepNumber, currentElement, updates);
        }
        //serialize regular properties
        public void SetNewGroupAttributes(
            XElement elementNeedingAttributes)
        {
            this.SetSharedObjectAttributes(string.Empty, elementNeedingAttributes);
            this.SetTotalCostsAttributes(string.Empty, elementNeedingAttributes);
            CalculatorHelpers.SetAttributeInt(elementNeedingAttributes,
                DataAppHelpers.Agreement.SERVICE_ID, this.ServiceId);
            CalculatorHelpers.SetAttribute(elementNeedingAttributes,
                DataAppHelpers.General.DOC_STATUS, this.DocStatus);
        }
        public void SetNewInputGroupAttributes(
           string attNameExt, ref XmlWriter writer)
        {
            //object only
            this.SetSharedObjectAttributes(attNameExt, ref writer);
            writer.WriteAttributeString(
                  string.Concat(DataAppHelpers.Agreement.SERVICE_ID, attNameExt), this.ServiceId.ToString());
            writer.WriteAttributeString(
                  string.Concat(DataAppHelpers.General.DOC_STATUS, attNameExt), this.DocStatus.ToString());
        }
    }
}
