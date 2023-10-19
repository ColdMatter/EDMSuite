using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections.Generic;


namespace AlFHardwareControl
{
    public class SimpleControlDesigner : ControlDesigner
    {
        public SimpleControlDesigner()
        {}

        protected override void PreFilterEvents(System.Collections.IDictionary properties)
        {
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            EventDescriptor pd = TypeDescriptor.CreateEvent(
                typeof(ParamSet),
                "OnSetClick",
                typeof(EventHandler),
                new Attribute[] { new DefaultEventAttribute("OnSetClick") });

            defaultValues.Add("OnSetClick", pd);

            ParamSet control1 = this.Component as ParamSet;
            control1.Label = control1.ToString().Split(" ".ToCharArray())[0];
        }


        
    }
}
