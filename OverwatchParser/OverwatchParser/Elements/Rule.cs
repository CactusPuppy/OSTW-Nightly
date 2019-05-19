﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverwatchParser.Elements
{
    public class Rule
    {
        private static int NumberOfRules = 0; // Required for navigating the ruleset.

        public string Name { get; private set; }
        public RuleEvent RuleEvent { get; private set; }
        public TeamSelector Team { get; private set; }
        public PlayerSelector Player { get; private set; }
        public bool IsGlobal { get; private set; }

        public Condition[] Conditions { get; set; }
        public Element[] Actions { get; set; }

        public Rule(string name, RuleEvent ruleEvent, TeamSelector team, PlayerSelector player) // Creates a rule.
        {
            if (name.Length > Constants.RULE_NAME_MAX_LENGTH)
                throw new ArgumentOutOfRangeException(nameof(name), name, $"Rule names cannot be longer than {Constants.RULE_NAME_MAX_LENGTH} characters.");

            Name = name;
            RuleEvent = ruleEvent;
            Team = team;
            Player = player;
            IsGlobal = ruleEvent == RuleEvent.Ongoing_Global;
        }

        public Rule(string name) // Creates a "Ongoing - Global" rule.
        {
            Name = name;
            RuleEvent = RuleEvent.Ongoing_Global;
            IsGlobal = true;
        }

        public void Input()
        {
            if (NumberOfRules == 0)
            {
                InputSim.Press(Keys.Tab, Wait.Short);
                InputSim.Repeat(Keys.Right, Wait.Short, 2);
            }

            // Create rule.
            InputSim.Press(Keys.Space, Wait.Long);

            NumberOfRules++;

            // Select rule name.
            InputSim.Repeat(Keys.Down, Wait.Short, NumberOfRules);

            InputSim.Press(Keys.Right, Wait.Short);

            // Input the name.
            InputSim.TextInput(Name, Wait.Short);

            // Leave name input menu.
            InputSim.Press(Keys.Tab, Wait.Short);

            // Leaving the input menu with tab resets the controller position.

            // Select the event type.
            InputSim.Repeat(Keys.Down, Wait.Short, NumberOfRules + 1);

            InputSim.SelectEnumMenuOption(RuleEvent);

            // If the rule's event is not global, set the team and player settings.
            if (RuleEvent != RuleEvent.Ongoing_Global)
            {
                // Set the team setting.
                InputSim.Press(Keys.Down, Wait.Short);
                InputSim.SelectEnumMenuOption(Team);

                // Set the player setting.
                InputSim.Press(Keys.Down, Wait.Short);
                InputSim.SelectEnumMenuOption(Player);
            }

            InputSim.Press(Keys.Down, Wait.Short); // Hovering over the "Add Action" button.

            InputSim.Press(Keys.Left, Wait.Short); // Hovering over the "Add Condition" button.

            // Add the conditions
            if (Conditions != null)
                foreach (Condition condition in Conditions)
                    condition.Input();

            // Select the "Add Action" button.
            InputSim.Press(Keys.Right, Wait.Short); // Hovering over the "Add Action" button.

            if (Actions != null)
                foreach(Element action in Actions)
                {
                    // Open the "Create Action" menu.
                    InputSim.Press(Keys.Space, Wait.Long);

                    // Setup control spot
                    InputSim.Press(Keys.Tab, Wait.Short);
                    // The spot will be at the bottom when tab is pressed. 
                    // Pressing up once will select the operator value, up another time will select the first value paramerer.
                    InputSim.Repeat(Keys.Up, Wait.Short, 3);

                    // Input value1.
                    action.Input();

                    // Close the Create Action menu.
                    InputSim.Press(Keys.Escape, Wait.Long);
                }

            // Close the rule
            InputSim.Repeat(Keys.Up, Wait.Short, 2);
            if (!IsGlobal)
                InputSim.Repeat(Keys.Up, Wait.Short, 2);

            InputSim.Press(Keys.Space, Wait.Short);

            InputSim.Repeat(Keys.Up, Wait.Short, NumberOfRules);
        }
    }
}
