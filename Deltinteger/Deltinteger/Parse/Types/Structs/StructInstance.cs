using System;
using System.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Deltin.Deltinteger.Parse
{
    public class StructInstance : CodeType, IAdditionalArray
    {
        public IVariableInstance[] Variables { get; private set; }
        private readonly IStructProvider _provider;
        private readonly Scope _objectScope;
        private bool _isReady;

        public StructInstance(IStructProvider provider, InstanceAnonymousTypeLinker genericsLinker) : base(provider.Name)
        {
            _provider = provider;
            _objectScope = new Scope("struct " + Name);

            provider.OnReady.OnReady(() => {
                Variables = new IVariableInstance[provider.Variables.Length];
                for (int i = 0; i < Variables.Length; i++)
                {
                    Variables[i] = provider.Variables[i].GetInstance(genericsLinker);
                    _objectScope.AddNativeVariable(Variables[i]);
                }
                _isReady = true;
            });
        }

        public override bool Is(CodeType other)
        {
            ThrowIfNotReady();

            if (Name != other.Name || Generics.Length != other.Generics.Length)
                return false;

            for (int i = 0; i < Generics.Length; i++)
                if (!Generics[i].Is(other.Generics[i]))
                    return false;

            return true;
        }

        public override bool Implements(CodeType type)
        {
            ThrowIfNotReady();

            foreach(var utype in type.UnionTypes())
            {
                if (!(utype is StructInstance other && other.Variables.Length == Variables.Length && Generics.Length == other.Generics.Length))
                    continue;
            
                for (int i = 0; i < Variables.Length; i++)
                {
                    var matchingVariable = other.Variables.FirstOrDefault(v => Variables[i].Name == v.Name);
                    if (matchingVariable == null || !Variables[i].CodeType.Implements(matchingVariable.CodeType))
                        continue;
                }
                
                return true;
            }
            return false;
        }

        public override Scope GetObjectScope()
        {
            ThrowIfNotReady();
            return _objectScope;
        }

        public override void AddObjectVariablesToAssigner(IWorkshopTree reference, VarIndexAssigner assigner)
        {
            var structValue = (IAssignedStructDictionary)reference;

            foreach (var variable in Variables)
                assigner.Add(variable.Provider, structValue[variable.Name]);
        }

        public override IWorkshopTree New(ActionSet actionSet, Constructor constructor, IWorkshopTree[] constructorValues, object[] additionalParameterData)
            => GetGettableAssigner(null).GetValue(new GettableAssignerValueInfo(actionSet)).GetVariable();

        public override IGettableAssigner GetGettableAssigner(IVariable variable) => new StructAssigner(this, ((Var)variable).InitialValue, false);
        IGettableAssigner IAdditionalArray.GetArrayAssigner(IVariable variable) => new StructAssigner(this, ((Var)variable).InitialValue, true);
        void IAdditionalArray.OverrideArray(ArrayType array) {}
        public override CompletionItem GetCompletion() => throw new System.NotImplementedException();

        void ThrowIfNotReady()
        {
            if (!_isReady) throw new Exception("You are but a fool.");
        }
    }
}