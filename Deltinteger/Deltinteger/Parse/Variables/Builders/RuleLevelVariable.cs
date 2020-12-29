using System;
using System.Linq;
using Deltin.Deltinteger.Parse.Variables.Build;

namespace Deltin.Deltinteger.Parse
{
    class RuleLevelVariable : VarBuilder
    {
        public RuleLevelVariable(Scope operationalScope, IVarContextHandler contextHandler) : base(operationalScope, contextHandler) {}

        protected override void CheckComponents()
        {
            RejectAttributes(new RejectAttributeComponent(AttributeType.Ref, AttributeType.Static));
            // Syntax error if both the globalvar and playervar attributes are missing.
            if (!_components.IsComponent<MacroComponent>() &&
                !_components.IsAttribute(AttributeType.GlobalVar) &&
                !_components.IsAttribute(AttributeType.PlayerVar))
                _diagnostics.Error("Expected the globalvar/playervar attribute.", _nameRange);
            
            RejectVirtualIfNotMacro();
        }

        protected override void Apply()
        {
            _varInfo.WholeContext = true;
            _varInfo.AccessLevel = AccessLevel.Public; // Set the access level.
            _varInfo.InitialValueResolve = InitialValueResolve.ApplyBlock; // Get the inital value after elements have been resolved.
            _varInfo.CodeLensType = CodeLensSourceType.RuleVariable; // Set the code lens type.
            _varInfo.HandleRestrictedCalls = true; // Handle restricted calls.
        }
    }
}