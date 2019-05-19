﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverwatchParser.Elements
{
    public class Condition
    {
        public Element Value1 { get; private set; }
        public Operators CompareOperator { get; private set; }
        public Element Value2 { get; private set; }

        public Condition(Element value1, Operators compareOperator = Operators.Equal, Element value2 = null)
        {
            if (value2 == null)
                value2 = new V_True();

            if (value1.ElementData.ElementType != ElementType.Value)
                throw new IncorrectElementTypeException(nameof(value1), true);

            if (value2.ElementData.ElementType != ElementType.Value)
                throw new IncorrectElementTypeException(nameof(value2), true);

            Value1 = value1;
            CompareOperator = compareOperator;
            Value2 = value2;
        }

        public void Input()
        {
            // Open the "Create Condition" menu.
            InputSim.Press(Keys.Space, Wait.Long);

            // Setup control spot
            InputSim.Press(Keys.Tab, Wait.Short);
            // The spot will be at the bottom when tab is pressed. 
            // Pressing up once will select the operator value, up another time will select the first value paramerer.
            InputSim.Repeat(Keys.Up, Wait.Short, 2);

            // Input value1.
            Value1.Input();

            // Set the operator.
            InputSim.Press(Keys.Down, Wait.Short);
            InputSim.SelectEnumMenuOption(CompareOperator);

            // Input value2.
            InputSim.Press(Keys.Down, Wait.Short);
            Value2.Input();

            // Close the Create Condition menu.
            InputSim.Press(Keys.Escape, Wait.Long);
        }
    }
}
