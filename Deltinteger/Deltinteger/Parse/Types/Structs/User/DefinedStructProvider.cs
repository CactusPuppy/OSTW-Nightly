using System;
using System.Collections.Generic;
using Deltin.Deltinteger.Compiler;
using Deltin.Deltinteger.Compiler.SyntaxTree;
using Deltin.Deltinteger.LanguageServer;

namespace Deltin.Deltinteger.Parse
{
    public class DefinedStructInitializer : StructInitializer, IDefinedTypeInitializer, IDeclarationKey, IGetMeta
    {
        public CodeType WorkingInstance { get; }
        public Location DefinedAt { get; }
        public Scope StaticScope { get; private set; }
        public Scope ObjectScope { get; private set; }
        readonly ParseInfo _parseInfo;
        readonly ClassContext _context;
        readonly Scope _scope;
        readonly VariableModifierGroup _contextualVariableModifiers = new VariableModifierGroup();

        public DefinedStructInitializer(ParseInfo parseInfo, Scope scope, ClassContext typeContext) : base(typeContext.Identifier.GetText())
        {
            _parseInfo = parseInfo;
            _context = typeContext;
            _scope = scope;
            DefinedAt = parseInfo.Script.GetLocation(typeContext.Identifier.GetRange(typeContext.Range));
            parseInfo.TranslateInfo.StagedInitiation.On(this);

            // Get the type args.
            GenericTypes = AnonymousType.GetGenerics(parseInfo, typeContext.Generics, this);

            // Add the declaration link.
            if (typeContext.Identifier)
            {
                parseInfo.Script.AddHover(
                    range: typeContext.Identifier.Range,
                    content: IDefinedTypeInitializer.Hover("struct", this));
                parseInfo.Script.Elements.AddDeclarationCall(this, new DeclarationCall(typeContext.Identifier.Range, true));
            }

            WorkingInstance = GetInstance();
        }

        public void GetMeta()
        {
            StaticScope = _scope.Child();
            ObjectScope = StaticScope.Child();

            // Add type args to scopes.
            foreach (var type in GenericTypes)
            {
                StaticScope.AddType(new GenericCodeTypeInitializer(type));
                ObjectScope.AddType(new GenericCodeTypeInitializer(type));
            }

            var declarationParseInfo = _parseInfo.SetContextualModifierGroup(_contextualVariableModifiers); 

            // Get declarations.
            foreach (var declaration in _context.Declarations)
            {
                var element = ((IDefinedTypeInitializer)this).ApplyDeclaration(declaration, declarationParseInfo);

                if (element is IMethodProvider method)
                    Methods.Add(method);
            }
        }

        public override StructInstance GetInstance() => new DefinedStructInstance(this, InstanceAnonymousTypeLinker.Empty);
        public override StructInstance GetInstance(InstanceAnonymousTypeLinker typeLinker) => new DefinedStructInstance(this, typeLinker);
        
        public override bool BuiltInTypeMatches(Type type) => false;
        public Scope GetObjectBasedScope() => ObjectScope;
        public Scope GetStaticBasedScope() => StaticScope;
        public IMethod GetOverridenFunction(DeltinScript deltinScript, FunctionOverrideInfo functionOverloadInfo) => throw new NotImplementedException();
        public IVariableInstance GetOverridenVariable(string variableName) => throw new NotImplementedException();
        public void AddObjectBasedScope(IMethod function) => ObjectScope.CopyMethod(function);
        public void AddStaticBasedScope(IMethod function) => StaticScope.CopyMethod(function);
        public void AddObjectBasedScope(IVariableInstance variable)
        {
            Variables.Add(variable.Provider);
            ObjectScope.CopyVariable(variable);
            _contextualVariableModifiers.MakeUnsettable(variable);
            variable.CodeType.GetCodeType(_parseInfo.TranslateInfo).TypeSemantics.MakeUnsettable(_parseInfo.TranslateInfo, _contextualVariableModifiers);
        }
        public void AddStaticBasedScope(IVariableInstance variable) => StaticScope.CopyVariable(variable);
        public override void Depend() => _parseInfo.TranslateInfo.StagedInitiation.Meta.Depend(this);
    }
}